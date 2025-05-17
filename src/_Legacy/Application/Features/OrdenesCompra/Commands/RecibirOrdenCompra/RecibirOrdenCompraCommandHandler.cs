using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.RecibirOrdenCompra
{
    public class RecibirOrdenCompraCommandHandler : IRequestHandler<RecibirOrdenCompraCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public RecibirOrdenCompraCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<bool> Handle(RecibirOrdenCompraCommand request, CancellationToken cancellationToken)
        {
            // Obtener la orden de compra y sus detalles
            var ordenCompra = await _context.OrdenesCompra
                .Include(o => o.Detalles)
                .Include(o => o.Proveedor)
                .FirstOrDefaultAsync(o => o.Id == request.OrdenCompraId, cancellationToken);

            if (ordenCompra == null)
            {
                throw new NotFoundException(nameof(OrdenCompra), request.OrdenCompraId);
            }

            // Comprobar que la orden esté en estado pendiente
            if (ordenCompra.Estado != EstadoOrdenCompra.Pendiente && ordenCompra.Estado != EstadoOrdenCompra.Parcial)
            {
                throw new InvalidOperationException($"No se puede recibir esta orden porque su estado actual es {ordenCompra.Estado}");
            }

            // Validar que los detalles de recepción correspondan a detalles de la orden
            var detallesOrdenIds = ordenCompra.Detalles.Select(d => d.Id).ToList();
            foreach (var detalleRecepcion in request.Detalles)
            {
                if (!detallesOrdenIds.Contains(detalleRecepcion.DetalleOrdenCompraId))
                {
                    throw new NotFoundException(
                        nameof(DetalleOrdenCompra), 
                        detalleRecepcion.DetalleOrdenCompraId);
                }
            }

            // Actualizar la orden de compra
            ordenCompra.FechaRecepcion = request.FechaRecepcion;
            
            if (!string.IsNullOrEmpty(request.Observaciones))
            {
                ordenCompra.Observaciones += $"\n[{_dateTime.Now}] Recepción: {request.Observaciones}";
            }

            // Procesar cada detalle de la recepción
            bool todosLosDetallesCompletados = true;
            
            foreach (var detalleRecepcion in request.Detalles)
            {
                var detalleOrden = ordenCompra.Detalles.First(d => d.Id == detalleRecepcion.DetalleOrdenCompraId);
                
                // Actualizar la cantidad recibida
                decimal cantidadAnterior = detalleOrden.CantidadRecibida;
                detalleOrden.CantidadRecibida += detalleRecepcion.CantidadRecibida;
                
                // Verificar si el detalle está completo
                detalleOrden.Completado = detalleOrden.CantidadRecibida >= detalleOrden.Cantidad;
                
                if (!detalleOrden.Completado)
                {
                    todosLosDetallesCompletados = false;
                }
                
                // Si hay inconformidad, registrar en observaciones
                if (detalleRecepcion.TieneInconformidad && !string.IsNullOrEmpty(detalleRecepcion.DescripcionInconformidad))
                {
                    detalleOrden.Observaciones += $"\n[{_dateTime.Now}] Inconformidad: {detalleRecepcion.DescripcionInconformidad}";
                }
                
                // Actualizar el inventario
                await ActualizarInventario(
                    detalleOrden.IngredienteId,
                    detalleOrden.UnidadMedida,
                    detalleRecepcion.CantidadRecibida,
                    detalleOrden.PrecioUnitario,
                    ordenCompra.NumeroOrden,
                    ordenCompra.Proveedor.Nombre,
                    cancellationToken);
            }

            // Determinar el estado final de la orden
            if (todosLosDetallesCompletados)
            {
                ordenCompra.Estado = EstadoOrdenCompra.Completada;
            }
            else if (request.EsRecepcionParcial)
            {
                ordenCompra.Estado = EstadoOrdenCompra.Parcial;
            }

            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }

        private async Task ActualizarInventario(
            int ingredienteId, 
            string unidadMedida, 
            decimal cantidad, 
            decimal costoUnitario,
            string numeroOrden,
            string nombreProveedor,
            CancellationToken cancellationToken)
        {
            // Buscar o crear registro en inventario
            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.IngredienteId == ingredienteId, cancellationToken);

            decimal cantidadAnterior = 0;
            
            if (inventario == null)
            {
                // Si no existe, crear nuevo registro
                inventario = new Domain.Entities.Inventario
                {
                    IngredienteId = ingredienteId,
                    CantidadDisponible = cantidad,
                    UnidadMedida = unidadMedida,
                    CostoUnitario = costoUnitario,
                    CantidadMinima = 0,
                    CantidadOptima = cantidad * 2, // Valor por defecto
                    Ubicacion = "Almacén principal",
                    Estado = "Activo",
                    UltimaActualizacion = _dateTime.Now,
                    UltimoInventarioPor = _currentUserService.UserId,
                    Observaciones = $"Registro creado por recepción de orden {numeroOrden}"
                };
                
                _context.Inventarios.Add(inventario);
                await _context.SaveChangesAsync(cancellationToken); // Guardar para obtener ID
            }
            else
            {
                cantidadAnterior = inventario.CantidadDisponible;
                
                // Calcular costo promedio ponderado
                decimal valorAnterior = cantidadAnterior * inventario.CostoUnitario;
                decimal valorNuevo = cantidad * costoUnitario;
                decimal cantidadTotal = cantidadAnterior + cantidad;
                
                if (cantidadTotal > 0) // Evitar división por cero
                {
                    inventario.CostoUnitario = (valorAnterior + valorNuevo) / cantidadTotal;
                }
                
                // Actualizar cantidad
                inventario.CantidadDisponible += cantidad;
                inventario.UltimaActualizacion = _dateTime.Now;
                inventario.UltimoInventarioPor = _currentUserService.UserId;
            }
            
            // Registrar el movimiento
            var movimiento = new MovimientoInventario
            {
                InventarioId = inventario.Id,
                IngredienteId = ingredienteId,
                TipoMovimiento = TipoMovimiento.Entrada,
                Cantidad = cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = inventario.CantidadDisponible,
                UnidadMedida = unidadMedida,
                CostoUnitario = costoUnitario,
                CostoTotal = cantidad * costoUnitario,
                Descripcion = $"Recepción de orden de compra {numeroOrden}",
                Referencia = numeroOrden,
                Fecha = _dateTime.Now,
                UsuarioId = _currentUserService.UserId
            };
            
            _context.MovimientosInventario.Add(movimiento);
        }
    }
} 