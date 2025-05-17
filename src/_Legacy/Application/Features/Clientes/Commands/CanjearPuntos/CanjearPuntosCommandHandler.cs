using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Commands.CanjearPuntos
{
    public class CanjearPuntosCommandHandler : IRequestHandler<CanjearPuntosCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public CanjearPuntosCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CanjearPuntosCommand request, CancellationToken cancellationToken)
        {
            // Validación básica
            if (request.Puntos <= 0)
            {
                throw new ValidationException("La cantidad de puntos a canjear debe ser mayor que cero.");
            }

            // Buscar el cliente con su tarjeta activa
            var cliente = await _context.Clientes
                .Include(c => c.TarjetasFidelizacion)
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                throw new NotFoundException(nameof(Cliente), request.ClienteId);
            }

            // Verificar si el cliente tiene una tarjeta activa
            var tarjeta = cliente.TarjetasFidelizacion
                .FirstOrDefault(t => t.Estado == EstadoTarjeta.Activa);

            if (tarjeta == null)
            {
                throw new ValidationException("El cliente no tiene una tarjeta de fidelización activa.");
            }

            // Verificar si tiene suficientes puntos disponibles
            if (tarjeta.PuntosDisponibles < request.Puntos)
            {
                throw new ValidationException($"El cliente no tiene suficientes puntos. Puntos disponibles: {tarjeta.PuntosDisponibles}, Puntos solicitados: {request.Puntos}");
            }

            // Si se proporciona un ID de promoción, verificar que existe y que requiere los puntos correctos
            if (request.PromocionId.HasValue)
            {
                var promocion = await _context.Promociones
                    .FirstOrDefaultAsync(p => p.Id == request.PromocionId.Value, cancellationToken);

                if (promocion == null)
                {
                    throw new NotFoundException(nameof(Promocion), request.PromocionId.Value);
                }

                if (!promocion.Activa)
                {
                    throw new ValidationException("La promoción no está activa.");
                }

                var fechaActual = _dateTime.Now.Date;
                if (promocion.FechaInicio > fechaActual || promocion.FechaFin < fechaActual)
                {
                    throw new ValidationException("La promoción no está vigente en la fecha actual.");
                }

                if (promocion.PuntosRequeridos != request.Puntos)
                {
                    throw new ValidationException($"La promoción requiere {promocion.PuntosRequeridos} puntos, pero se están canjeando {request.Puntos}.");
                }

                // Registrar el uso de la promoción
                promocion.VecesUsada++;
                cliente.PromocionesUsadas.Add(promocion);
            }

            // Actualizar la tarjeta
            var fechaOperacion = _dateTime.Now;
            tarjeta.PuntosDisponibles -= request.Puntos;
            tarjeta.PuntosCanjeados += request.Puntos;
            
            // Actualizar el cliente
            cliente.PuntosRedimidos += request.Puntos;
            cliente.UltimaVisita = fechaOperacion;
            
            // Registrar el movimiento de puntos
            var historialPuntos = new HistorialPuntos
            {
                TarjetaFidelizacionId = tarjeta.Id,
                ClienteId = cliente.Id,
                TipoMovimiento = TipoMovimientoPuntos.Canje,
                Puntos = request.Puntos,
                Descripcion = request.Descripcion ?? "Canje de puntos por promoción",
                Fecha = fechaOperacion,
                ComandaId = request.ComandaId,
                PromocionId = request.PromocionId,
                UsuarioId = _currentUserService.UserId
            };

            _context.HistorialPuntos.Add(historialPuntos);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 