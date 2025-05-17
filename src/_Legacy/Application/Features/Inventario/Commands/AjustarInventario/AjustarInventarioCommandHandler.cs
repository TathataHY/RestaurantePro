using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Inventario.Commands.AjustarInventario
{
    public class AjustarInventarioCommandHandler : IRequestHandler<AjustarInventarioCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public AjustarInventarioCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(AjustarInventarioCommand request, CancellationToken cancellationToken)
        {
            // Buscar el registro de inventario
            Domain.Entities.Inventario inventario = null;
            
            if (request.InventarioId > 0)
            {
                inventario = await _context.Inventarios
                    .Include(i => i.Ingrediente)
                    .FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
                
                if (inventario == null)
                {
                    throw new NotFoundException(nameof(Domain.Entities.Inventario), request.InventarioId);
                }
            }
            else if (request.IngredienteId.HasValue)
            {
                // Buscar por ingrediente
                inventario = await _context.Inventarios
                    .Include(i => i.Ingrediente)
                    .FirstOrDefaultAsync(i => i.IngredienteId == request.IngredienteId.Value, cancellationToken);
                
                // Si no existe, verificar que exista el ingrediente y crear un nuevo registro
                if (inventario == null)
                {
                    var ingrediente = await _context.Ingredientes
                        .FirstOrDefaultAsync(i => i.Id == request.IngredienteId.Value, cancellationToken);
                    
                    if (ingrediente == null)
                    {
                        throw new NotFoundException(nameof(Ingrediente), request.IngredienteId.Value);
                    }
                    
                    // Crear un nuevo registro de inventario
                    inventario = new Domain.Entities.Inventario
                    {
                        IngredienteId = ingrediente.Id,
                        Ingrediente = ingrediente,
                        CantidadDisponible = 0,
                        UnidadMedida = ingrediente.UnidadMedida,
                        CostoUnitario = request.CostoUnitario ?? 0,
                        CantidadMinima = 0,
                        CantidadOptima = 0,
                        Ubicacion = "Almacén principal",
                        Estado = "Activo",
                        UltimaActualizacion = _dateTime.Now,
                        UltimoInventarioPor = _currentUserService.UserId,
                        Observaciones = "Registro creado automáticamente por ajuste"
                    };
                    
                    _context.Inventarios.Add(inventario);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                throw new ValidationException("Debe especificar el ID del inventario o el ID del ingrediente");
            }
            
            // Calcular cantidades para el movimiento
            decimal cantidadAnterior = inventario.CantidadDisponible;
            decimal cantidadNueva;
            
            switch (request.TipoMovimiento)
            {
                case TipoMovimiento.Entrada:
                    cantidadNueva = cantidadAnterior + request.Cantidad;
                    break;
                case TipoMovimiento.Salida:
                case TipoMovimiento.Merma:
                    if (request.Cantidad > cantidadAnterior)
                    {
                        throw new ValidationException($"No hay suficiente stock disponible. Stock actual: {cantidadAnterior}");
                    }
                    cantidadNueva = cantidadAnterior - request.Cantidad;
                    break;
                case TipoMovimiento.Ajuste:
                    // En caso de ajuste directo, la cantidad solicitada es la cantidad final
                    cantidadNueva = request.Cantidad;
                    break;
                default:
                    throw new ValidationException($"Tipo de movimiento no válido: {request.TipoMovimiento}");
            }
            
            // Actualizar el costo unitario si se proporciona
            if (request.CostoUnitario.HasValue && request.TipoMovimiento == TipoMovimiento.Entrada)
            {
                // Si es una entrada, podemos actualizar el costo promedio
                decimal valorAnterior = cantidadAnterior * inventario.CostoUnitario;
                decimal valorNuevo = request.Cantidad * request.CostoUnitario.Value;
                decimal valorTotal = valorAnterior + valorNuevo;
                
                if (cantidadNueva > 0)
                {
                    inventario.CostoUnitario = valorTotal / cantidadNueva;
                }
            }
            
            // Actualizar el inventario
            inventario.CantidadDisponible = cantidadNueva;
            inventario.UltimaActualizacion = _dateTime.Now;
            inventario.UltimoInventarioPor = _currentUserService.UserId;
            
            if (!string.IsNullOrEmpty(request.Motivo))
            {
                inventario.Observaciones = request.Motivo;
            }
            
            // Registrar el movimiento
            var movimiento = new MovimientoInventario
            {
                InventarioId = inventario.Id,
                IngredienteId = inventario.IngredienteId,
                TipoMovimiento = request.TipoMovimiento,
                Cantidad = request.Cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = cantidadNueva,
                UnidadMedida = inventario.UnidadMedida,
                CostoUnitario = request.CostoUnitario ?? inventario.CostoUnitario,
                CostoTotal = request.Cantidad * (request.CostoUnitario ?? inventario.CostoUnitario),
                Descripcion = request.Motivo,
                Referencia = request.Referencia,
                Fecha = _dateTime.Now,
                UsuarioId = _currentUserService.UserId
            };
            
            _context.MovimientosInventario.Add(movimiento);
            
            // Guardar los cambios
            await _context.SaveChangesAsync(cancellationToken);
            
            return movimiento.Id;
        }
    }
} 