using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Comandas.Commands.CrearComanda
{
    public class CrearComandaCommandHandler : IRequestHandler<CrearComandaCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CrearComandaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> Handle(CrearComandaCommand request, CancellationToken cancellationToken)
        {
            // Verificar que la mesa exista
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.Id == request.MesaId, cancellationToken);

            if (mesa == null)
                throw new Exception($"No existe una mesa con el ID {request.MesaId}");

            // Verificar que la mesa esté disponible
            if (mesa.Estado != EstadoMesa.Libre)
                throw new Exception($"La mesa {mesa.Numero} no está disponible");

            // Obtener el ID del usuario actual
            var usuarioId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(usuarioId))
                throw new Exception("No se ha podido identificar al usuario");

            // Crear la comanda
            var comanda = new Comanda
            {
                MesaId = request.MesaId,
                UsuarioId = usuarioId,
                NumeroComanda = GenerarNumeroComanda(),
                Estado = EstadoComanda.Pendiente,
                Notas = request.Notas,
                FechaCreacion = DateTime.Now
            };

            _context.Comandas.Add(comanda);
            await _context.SaveChangesAsync(cancellationToken);

            // Cambiar el estado de la mesa a ocupada
            mesa.Estado = EstadoMesa.Ocupada;
            await _context.SaveChangesAsync(cancellationToken);

            // Crear los detalles de la comanda
            if (request.Detalles != null && request.Detalles.Any())
            {
                decimal subtotal = 0;

                foreach (var detalleDto in request.Detalles)
                {
                    // Obtener el producto
                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId, cancellationToken);

                    if (producto == null)
                        throw new Exception($"No existe un producto con el ID {detalleDto.ProductoId}");

                    if (!producto.Disponible)
                        throw new Exception($"El producto {producto.Nombre} no está disponible");

                    // Calcular el subtotal del detalle
                    decimal subtotalDetalle = producto.Precio * detalleDto.Cantidad;
                    subtotal += subtotalDetalle;

                    // Crear el detalle
                    var detalle = new ComandaDetalle
                    {
                        ComandaId = comanda.Id,
                        ProductoId = producto.Id,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalDetalle,
                        Estado = EstadoComandaDetalle.Pendiente,
                        NotasEspeciales = detalleDto.NotasEspeciales,
                        FechaCreacion = DateTime.Now
                    };

                    _context.ComandaDetalles.Add(detalle);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Crear las personalizaciones del detalle
                    if (detalleDto.Personalizaciones != null && detalleDto.Personalizaciones.Any())
                    {
                        foreach (var personalizacionDto in detalleDto.Personalizaciones)
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
                }

                // Actualizar los totales de la comanda
                comanda.Subtotal = subtotal;
                comanda.Impuestos = CalcularImpuestos(subtotal);
                comanda.Total = comanda.Subtotal + comanda.Impuestos;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return comanda.Id;
        }

        private string GenerarNumeroComanda()
        {
            // Generar un número único para la comanda
            // Formato: CMD-YYYYMMDD-XXXX donde XXXX es un número secuencial
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            
            // Obtener el último número de comanda para hoy
            var ultimaComanda = _context.Comandas
                .Where(c => c.NumeroComanda.StartsWith($"CMD-{fecha}"))
                .OrderByDescending(c => c.NumeroComanda)
                .FirstOrDefault();

            int secuencial = 1;
            if (ultimaComanda != null)
            {
                string[] partes = ultimaComanda.NumeroComanda.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int ultimoSecuencial))
                {
                    secuencial = ultimoSecuencial + 1;
                }
            }

            return $"CMD-{fecha}-{secuencial:D4}";
        }

        private decimal CalcularImpuestos(decimal subtotal)
        {
            // Por ahora, aplicamos un IVA fijo del 16%
            // En un sistema real, esto podría ser más complejo y configurable
            return Math.Round(subtotal * 0.16m, 2);
        }
    }
} 