using System;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs
{
    /// <summary>
    /// DTO para representar una Reservación en la capa de aplicación
    /// </summary>
    public class ReservacionDto
    {
        /// <summary>
        /// Identificador único de la reservación
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID del cliente que realiza la reservación
        /// </summary>
        public Guid ClienteId { get; set; }
        
        /// <summary>
        /// Nombre del cliente (para mostrar en interfaces)
        /// </summary>
        public string? NombreCliente { get; set; }
        
        /// <summary>
        /// Teléfono de contacto del cliente
        /// </summary>
        public string? TelefonoContacto { get; set; }
        
        /// <summary>
        /// Email de contacto del cliente
        /// </summary>
        public string? EmailContacto { get; set; }
        
        /// <summary>
        /// Fecha y hora de la reservación
        /// </summary>
        public DateTime FechaReservacion { get; set; }
        
        /// <summary>
        /// Duración estimada de la reservación
        /// </summary>
        public TimeSpan DuracionEstimada { get; set; }
        
        /// <summary>
        /// Cantidad de comensales
        /// </summary>
        public int CantidadComensales { get; set; }
        
        /// <summary>
        /// ID de la mesa reservada
        /// </summary>
        public Guid MesaId { get; set; }
        
        /// <summary>
        /// Número de la mesa (para mostrar en interfaces)
        /// </summary>
        public int? NumeroMesa { get; set; }
        
        /// <summary>
        /// Estado actual de la reservación
        /// </summary>
        public EstadoReservacion Estado { get; set; }
        
        /// <summary>
        /// Nombre del estado para mostrar en interfaces
        /// </summary>
        public string EstadoNombre => Estado.ToString();
        
        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        public string? Observaciones { get; set; }
        
        /// <summary>
        /// Fecha de creación de la reservación
        /// </summary>
        public DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }
        
        /// <summary>
        /// Indica si se ha enviado confirmación al cliente
        /// </summary>
        public bool ConfirmacionEnviada { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se envió la confirmación
        /// </summary>
        public DateTime? FechaConfirmacionEnviada { get; set; }
        
        /// <summary>
        /// Ocasión especial de la reservación (aniversario, cumpleaños, etc.)
        /// </summary>
        public string? OcasionEspecial { get; set; }
        
        /// <summary>
        /// Solicitudes especiales del cliente
        /// </summary>
        public string? SolicitudesEspeciales { get; set; }
        
        /// <summary>
        /// Código de confirmación enviado al cliente
        /// </summary>
        public string? CodigoConfirmacion { get; set; }
    }
} 