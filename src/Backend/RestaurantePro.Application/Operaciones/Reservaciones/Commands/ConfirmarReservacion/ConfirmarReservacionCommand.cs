using System;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion
{
    /// <summary>
    /// Comando para confirmar una reservación existente
    /// </summary>
    public class ConfirmarReservacionCommand : IRequest<Result<ReservacionDto>>
    {
        /// <summary>
        /// ID de la reservación a confirmar
        /// </summary>
        public Guid ReservacionId { get; set; }
        
        /// <summary>
        /// Opcional: Código de confirmación enviado al cliente
        /// </summary>
        public string? CodigoConfirmacion { get; set; }
        
        /// <summary>
        /// Opcional: Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
    }
} 