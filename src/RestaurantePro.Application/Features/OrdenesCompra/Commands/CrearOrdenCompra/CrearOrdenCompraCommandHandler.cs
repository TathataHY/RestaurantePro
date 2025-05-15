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

namespace RestaurantePro.Application.Features.OrdenesCompra.Commands.CrearOrdenCompra
{
    public class CrearOrdenCompraCommandHandler : IRequestHandler<CrearOrdenCompraCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public CrearOrdenCompraCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<int> Handle(CrearOrdenCompraCommand request, CancellationToken cancellationToken)
        {
            // Validar que exista el proveedor
            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == request.ProveedorId, cancellationToken);

            if (proveedor == null)
            {
                throw new NotFoundException(nameof(Proveedor), request.ProveedorId);
            }

            // Validar que los ingredientes existan
            var ingredienteIds = request.Detalles.Select(d => d.IngredienteId).ToList();
            var ingredientes = await _context.Ingredientes
                .Where(i => ingredienteIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, cancellationToken);

            foreach (var detalleDto in request.Detalles)
            {
                if (!ingredientes.ContainsKey(detalleDto.IngredienteId))
                {
                    throw new NotFoundException(nameof(Ingrediente), detalleDto.IngredienteId);
                }
            }

            // Generar número secuencial para la orden
            int ultimoNumero = await _context.OrdenesCompra
                .OrderByDescending(o => o.NumeroOrden)
                .Select(o => o.NumeroOrden)
                .FirstOrDefaultAsync(cancellationToken);

            string numeroOrden = $"OC-{DateTime.Now.Year}-{++ultimoNumero:D5}";

            // Crear la orden de compra
            var ordenCompra = new OrdenCompra
            {
                NumeroOrden = numeroOrden,
                ProveedorId = request.ProveedorId,
                FechaOrden = _dateTime.Now,
                FechaEntregaEstimada = request.FechaEntregaEstimada,
                Estado = EstadoOrdenCompra.Pendiente,
                Observaciones = request.Observaciones,
                UsuarioId = _currentUserService.UserId,
                Descuento = request.Descuento
            };

            // Agregar los detalles
            decimal subtotal = 0;
            decimal impuestos = 0;
            decimal descuentoTotal = 0;

            foreach (var detalleDto in request.Detalles)
            {
                // Calcular valores del detalle
                decimal subtotalDetalle = detalleDto.Cantidad * detalleDto.PrecioUnitario;
                decimal descuentoDetalle = subtotalDetalle * (detalleDto.PorcentajeDescuento / 100);
                decimal impuestoDetalle = (subtotalDetalle - descuentoDetalle) * (detalleDto.PorcentajeImpuesto / 100);
                decimal totalDetalle = subtotalDetalle - descuentoDetalle + impuestoDetalle;

                var detalle = new DetalleOrdenCompra
                {
                    IngredienteId = detalleDto.IngredienteId,
                    Cantidad = detalleDto.Cantidad,
                    UnidadMedida = detalleDto.UnidadMedida,
                    PrecioUnitario = detalleDto.PrecioUnitario,
                    Subtotal = subtotalDetalle,
                    Impuesto = impuestoDetalle,
                    Descuento = descuentoDetalle,
                    Total = totalDetalle,
                    CantidadRecibida = 0,
                    Completado = false,
                    Observaciones = detalleDto.Observaciones
                };

                ordenCompra.Detalles.Add(detalle);

                // Acumular totales
                subtotal += subtotalDetalle;
                impuestos += impuestoDetalle;
                descuentoTotal += descuentoDetalle;
            }

            // Calcular totales de la orden
            ordenCompra.Subtotal = subtotal;
            ordenCompra.Impuestos = impuestos;
            ordenCompra.Descuento = subtotal * (request.Descuento / 100) + descuentoTotal;
            ordenCompra.Total = subtotal - ordenCompra.Descuento + impuestos;

            _context.OrdenesCompra.Add(ordenCompra);
            await _context.SaveChangesAsync(cancellationToken);

            return ordenCompra.Id;
        }
    }
} 