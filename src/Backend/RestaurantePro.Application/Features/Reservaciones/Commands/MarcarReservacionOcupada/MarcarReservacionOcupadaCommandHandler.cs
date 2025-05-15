using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.MarcarReservacionOcupada
{
    public class MarcarReservacionOcupadaCommandHandler : IRequestHandler<MarcarReservacionOcupadaCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public MarcarReservacionOcupadaCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(MarcarReservacionOcupadaCommand request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservacion == null)
            {
                throw new NotFoundException("Reservación", request.Id);
            }

            // Solo se puede ocupar una reservación confirmada
            if (reservacion.Estado != EstadoReservacion.Confirmada)
            {
                throw new ValidationException($"No se puede ocupar una reservación en estado {reservacion.Estado}");
            }

            // Verificar que la mesa esté disponible
            if (reservacion.Mesa.Estado != EstadoMesa.Disponible)
            {
                throw new ValidationException($"La mesa {reservacion.Mesa.Numero} no está disponible actualmente");
            }

            // Actualizar el estado de la reservación
            reservacion.Estado = EstadoReservacion.Ocupada;
            
            // Actualizar el número real de personas si se proporcionó
            if (request.NumeroPersonasReal.HasValue)
            {
                reservacion.NumeroPersonas = request.NumeroPersonasReal.Value;
            }
            
            // Añadir notas si se proporcionaron
            if (!string.IsNullOrEmpty(request.Notas))
            {
                reservacion.Notas = string.IsNullOrEmpty(reservacion.Notas)
                    ? request.Notas
                    : $"{reservacion.Notas}\n\n{request.Notas}";
            }
            
            // Actualizar metadatos
            reservacion.UltimaModificacion = DateTime.Now;
            reservacion.UltimoUsuarioModificacionId = _currentUserService.UserId;
            
            // Actualizar el estado de la mesa
            reservacion.Mesa.Estado = EstadoMesa.Ocupada;
            reservacion.Mesa.UltimaModificacion = DateTime.Now;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 