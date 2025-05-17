using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Clientes.Commands.AcumularPuntos
{
    public class AcumularPuntosCommandHandler : IRequestHandler<AcumularPuntosCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUserService _currentUserService;

        public AcumularPuntosCommandHandler(
            IApplicationDbContext context,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _dateTime = dateTime;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(AcumularPuntosCommand request, CancellationToken cancellationToken)
        {
            // Validación básica
            if (request.Puntos <= 0)
            {
                throw new ValidationException("La cantidad de puntos debe ser mayor que cero.");
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

            // Actualizar la tarjeta
            var fechaActual = _dateTime.Now;
            tarjeta.PuntosAcumulados += request.Puntos;
            tarjeta.PuntosDisponibles += request.Puntos;
            
            // Actualizar el cliente
            cliente.PuntosAcumulados += request.Puntos;
            cliente.UltimaVisita = fechaActual;
            
            // Registrar el movimiento de puntos
            var historialPuntos = new HistorialPuntos
            {
                TarjetaFidelizacionId = tarjeta.Id,
                ClienteId = cliente.Id,
                TipoMovimiento = TipoMovimientoPuntos.Acumulacion,
                Puntos = request.Puntos,
                Descripcion = request.Descripcion ?? "Acumulación de puntos por consumo",
                Fecha = fechaActual,
                ComandaId = request.ComandaId,
                UsuarioId = _currentUserService.UserId
            };

            _context.HistorialPuntos.Add(historialPuntos);
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 