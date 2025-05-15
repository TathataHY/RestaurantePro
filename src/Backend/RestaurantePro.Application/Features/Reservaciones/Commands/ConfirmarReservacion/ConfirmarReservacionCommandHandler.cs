using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.ConfirmarReservacion
{
    public class ConfirmarReservacionCommandHandler : IRequestHandler<ConfirmarReservacionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public ConfirmarReservacionCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (reservacion == null)
            {
                throw new NotFoundException("Reservación", request.Id);
            }

            if (reservacion.Estado != EstadoReservacion.Pendiente)
            {
                throw new ValidationException($"No se puede confirmar una reservación en estado {reservacion.Estado}");
            }

            reservacion.Estado = EstadoReservacion.Confirmada;
            
            if (!string.IsNullOrEmpty(request.NotasConfirmacion))
            {
                reservacion.Notas = string.IsNullOrEmpty(reservacion.Notas)
                    ? request.NotasConfirmacion
                    : $"{reservacion.Notas}\n\nConfirmación: {request.NotasConfirmacion}";
            }
            
            reservacion.UltimaModificacion = DateTime.Now;
            reservacion.UltimoUsuarioModificacionId = _currentUserService.UserId;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 