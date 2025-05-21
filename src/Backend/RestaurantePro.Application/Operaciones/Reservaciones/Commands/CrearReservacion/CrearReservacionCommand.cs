using System;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion
{
    /// <summary>
    /// Comando para crear una nueva reservación
    /// </summary>
    public class CrearReservacionCommand : IRequest<Result<ReservacionDto>>
    {
        /// <summary>
        /// ID del cliente que realiza la reservación
        /// </summary>
        public Guid ClienteId { get; set; }
        
        /// <summary>
        /// Fecha y hora de la reservación
        /// </summary>
        public DateTime FechaReservacion { get; set; }
        
        /// <summary>
        /// Duración estimada en minutos
        /// </summary>
        public int DuracionMinutos { get; set; } = 90;
        
        /// <summary>
        /// Cantidad de comensales
        /// </summary>
        public int CantidadComensales { get; set; }
        
        /// <summary>
        /// ID de la mesa a reservar
        /// Si no se especifica, el sistema buscará una mesa disponible
        /// </summary>
        public Guid? MesaId { get; set; }
        
        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        public string Telefono { get; set; } = string.Empty;
        
        /// <summary>
        /// Email de contacto
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Ocasión especial (aniversario, cumpleaños, etc.)
        /// </summary>
        public string? OcasionEspecial { get; set; }
        
        /// <summary>
        /// Solicitudes especiales
        /// </summary>
        public string? SolicitudesEspeciales { get; set; }
    }
} 