using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Inventario.Commands.RegistrarMovimiento
{
    public class RegistrarMovimientoCommandHandler : IRequestHandler<RegistrarMovimientoCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public RegistrarMovimientoCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(RegistrarMovimientoCommand request, CancellationToken cancellationToken)
        {
            // Buscar el registro de inventario
            var inventario = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
            
            if (inventario == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Inventario), request.InventarioId);
            }
            
            decimal cantidadAnterior = inventario.CantidadDisponible;
            decimal cantidadNueva;
            
            // Calcular la nueva cantidad según el tipo de movimiento
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
            
            // Actualizar el inventario
            inventario.CantidadDisponible = cantidadNueva;
            inventario.UltimaActualizacion = _dateTime.Now;
            inventario.UltimoInventarioPor = _currentUserService.UserId;
            
            // Crear el movimiento
            var movimiento = new MovimientoInventario
            {
                InventarioId = inventario.Id,
                IngredienteId = inventario.IngredienteId,
                TipoMovimiento = request.TipoMovimiento,
                Cantidad = request.Cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = cantidadNueva,
                UnidadMedida = inventario.UnidadMedida,
                CostoUnitario = inventario.CostoUnitario,
                CostoTotal = request.Cantidad * inventario.CostoUnitario,
                Descripcion = request.Descripcion,
                Referencia = request.Referencia,
                Fecha = _dateTime.Now,
                ComandaId = request.ComandaId,
                OrdenCompraId = request.OrdenCompraId,
                UsuarioId = _currentUserService.UserId
            };
            
            _context.MovimientosInventario.Add(movimiento);
            
            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);
            
            return movimiento.Id;
        }
    }
} 