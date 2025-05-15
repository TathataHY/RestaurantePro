using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Comandas.Commands.AgregarProductoComanda
{
    public class AgregarProductoComandaCommandHandler : IRequestHandler<AgregarProductoComandaCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public AgregarProductoComandaCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AgregarProductoComandaCommand request, CancellationToken cancellationToken)
        {
            // Verificar que la comanda exista
            var comanda = await _context.Comandas
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId, cancellationToken);

            if (comanda == null)
                throw new Exception($"No existe una comanda con el ID {request.ComandaId}");

            // Verificar que la comanda no esté en estado pagada o cancelada
            if (comanda.Estado == EstadoComanda.Pagada || comanda.Estado == EstadoComanda.Cancelada)
                throw new Exception($"No se pueden agregar productos a una comanda {comanda.Estado}");

            // Verificar que el producto exista
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.ProductoId, cancellationToken);

            if (producto == null)
                throw new Exception($"No existe un producto con el ID {request.ProductoId}");

            if (!producto.Disponible)
                throw new Exception($"El producto {producto.Nombre} no está disponible");

            // Calcular el subtotal del detalle
            decimal subtotalDetalle = producto.Precio * request.Cantidad;

            // Crear el detalle de comanda
            var detalle = new ComandaDetalle
            {
                ComandaId = comanda.Id,
                ProductoId = producto.Id,
                Cantidad = request.Cantidad,
                PrecioUnitario = producto.Precio,
                Subtotal = subtotalDetalle,
                Estado = EstadoComandaDetalle.Pendiente,
                NotasEspeciales = request.NotasEspeciales,
                FechaCreacion = DateTime.Now
            };

            _context.ComandaDetalles.Add(detalle);
            await _context.SaveChangesAsync(cancellationToken);

            // Crear personalizaciones si existen
            if (request.Personalizaciones != null && request.Personalizaciones.Any())
            {
                foreach (var personalizacionDto in request.Personalizaciones)
                {
                    // Verificar que el ingrediente exista
                    var ingrediente = await _context.Ingredientes
                        .FirstOrDefaultAsync(i => i.Id == personalizacionDto.IngredienteId, cancellationToken);

                    if (ingrediente == null)
                        throw new Exception($"No existe un ingrediente con el ID {personalizacionDto.IngredienteId}");

                    // Crear la personalización
                    var personalizacion = new ComandaDetallePersonalizacion
                    {
                        ComandaDetalleId = detalle.Id,
                        IngredienteId = ingrediente.Id,
                        Accion = (AccionPersonalizacion)personalizacionDto.AccionPersonalizacion,
                        Cantidad = personalizacionDto.Cantidad,
                        FechaCreacion = DateTime.Now
                    };

                    _context.ComandaDetallePersonalizaciones.Add(personalizacion);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }

            // Actualizar los totales de la comanda
            var detalles = await _context.ComandaDetalles
                .Where(d => d.ComandaId == comanda.Id)
                .ToListAsync(cancellationToken);

            comanda.Subtotal = detalles.Sum(d => d.Subtotal);
            comanda.Impuestos = CalcularImpuestos(comanda.Subtotal);
            comanda.Total = comanda.Subtotal + comanda.Impuestos;
            comanda.UltimaModificacion = DateTime.Now;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private decimal CalcularImpuestos(decimal subtotal)
        {
            // Por ahora, aplicamos un IVA fijo del 16%
            // En un sistema real, esto podría ser más complejo y configurable
            return Math.Round(subtotal * 0.16m, 2);
        }
    }
} 