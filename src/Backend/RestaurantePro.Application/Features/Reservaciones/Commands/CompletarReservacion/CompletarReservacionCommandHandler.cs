using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Exceptions;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reservaciones.Commands.CompletarReservacion
{
    public class CompletarReservacionCommandHandler : IRequestHandler<CompletarReservacionCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public CompletarReservacionCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(CompletarReservacionCommand request, CancellationToken cancellationToken)
        {
            var reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (reservacion == null)
            {
                throw new NotFoundException("Reservación", request.Id);
            }

            // Solo se puede completar una reservación que esté ocupada
            if (reservacion.Estado != EstadoReservacion.Ocupada)
            {
                throw new ValidationException($"No se puede completar una reservación en estado {reservacion.Estado}");
            }

            // Actualizar el estado de la reservación
            reservacion.Estado = EstadoReservacion.Completada;
            
            // Añadir comentarios si se proporcionaron
            if (!string.IsNullOrEmpty(request.Comentarios))
            {
                reservacion.Notas = string.IsNullOrEmpty(reservacion.Notas)
                    ? $"Comentarios finales: {request.Comentarios}"
                    : $"{reservacion.Notas}\n\nComentarios finales: {request.Comentarios}";
            }
            
            // Actualizar metadatos
            reservacion.UltimaModificacion = DateTime.Now;
            reservacion.UltimoUsuarioModificacionId = _currentUserService.UserId;
            
            // Liberar la mesa
            reservacion.Mesa.Estado = EstadoMesa.Disponible;
            reservacion.Mesa.UltimaModificacion = DateTime.Now;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
} 