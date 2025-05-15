using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CancelarReservacion
{
    public class CancelarReservacionCommandHandler : IRequestHandler<CancelarReservacionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CancelarReservacionCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CancelarReservacionCommand request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (reservacion == null)
            {
                throw new NotFoundException("Reservación", request.Id);
            }

            // Solo se pueden cancelar reservaciones pendientes o confirmadas
            if (reservacion.Estado != EstadoReservacion.Pendiente && 
                reservacion.Estado != EstadoReservacion.Confirmada)
            {
                throw new ValidationException($"No se puede cancelar una reservación en estado {reservacion.Estado}");
            }

            reservacion.Estado = EstadoReservacion.Cancelada;
            
            if (!string.IsNullOrEmpty(request.MotivoCancelacion))
            {
                reservacion.Notas = string.IsNullOrEmpty(reservacion.Notas)
                    ? $"Motivo de cancelación: {request.MotivoCancelacion}"
                    : $"{reservacion.Notas}\n\nMotivo de cancelación: {request.MotivoCancelacion}";
            }
            
            reservacion.UltimaModificacion = DateTime.Now;
            reservacion.UltimoUsuarioModificacionId = _currentUserService.UserId;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 