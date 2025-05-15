using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.MarcarNoShow
{
    public class MarcarNoShowCommandHandler : IRequestHandler<MarcarNoShowCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public MarcarNoShowCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _context = context;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<bool> Handle(MarcarNoShowCommand request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservacion == null)
            {
                throw new NotFoundException("Reservación", request.Id);
            }

            // Solo se puede marcar como no show una reservación confirmada
            if (reservacion.Estado != EstadoReservacion.Confirmada)
            {
                throw new ValidationException($"No se puede marcar como no show una reservación en estado {reservacion.Estado}");
            }

            // Verificar que la fecha de la reservación haya pasado (opcional, se puede quitar)
            var fechaHoraActual = _dateTime.Now;
            var fechaHoraReservacion = reservacion.FechaReservacion;
            
            // Solo se puede marcar como no show 30 minutos después de la hora de la reservación
            if (fechaHoraActual < fechaHoraReservacion.AddMinutes(30))
            {
                throw new ValidationException("Solo se puede marcar como no show 30 minutos después de la hora de la reservación");
            }

            // Actualizar el estado de la reservación
            reservacion.Estado = EstadoReservacion.NoShow;
            
            // Añadir observaciones si se proporcionaron
            if (!string.IsNullOrEmpty(request.Observaciones))
            {
                reservacion.Notas = string.IsNullOrEmpty(reservacion.Notas)
                    ? $"No show: {request.Observaciones}"
                    : $"{reservacion.Notas}\n\nNo show: {request.Observaciones}";
            }
            
            // Actualizar metadatos
            reservacion.UltimaModificacion = fechaHoraActual;
            reservacion.UltimoUsuarioModificacionId = _currentUserService.UserId;
            
            // Asegurarse de que la mesa esté disponible
            if (reservacion.Mesa.Estado != EstadoMesa.Disponible)
            {
                reservacion.Mesa.Estado = EstadoMesa.Disponible;
                reservacion.Mesa.UltimaModificacion = fechaHoraActual;
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 