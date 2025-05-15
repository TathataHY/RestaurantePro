using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de reservación
    /// </summary>
    public class ReservacionDto
    {
        /// <summary>
        /// ID de la reservación
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la mesa
        /// </summary>
        public int MesaId { get; set; }

        /// <summary>
        /// Número de la mesa
        /// </summary>
        public int NumeroMesa { get; set; }

        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string NombreCliente { get; set; }

        /// <summary>
        /// Teléfono del cliente
        /// </summary>
        public string Telefono { get; set; }

        /// <summary>
        /// Email del cliente
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Fecha y hora de la reservación
        /// </summary>
        public DateTime FechaReservacion { get; set; }

        /// <summary>
        /// Duración estimada en minutos
        /// </summary>
        public int DuracionEstimada { get; set; }

        /// <summary>
        /// Número de personas
        /// </summary>
        public int NumeroPersonas { get; set; }

        /// <summary>
        /// Estado de la reservación
        /// </summary>
        public EstadoReservacion Estado { get; set; }

        /// <summary>
        /// Nombre del estado para mostrar
        /// </summary>
        public string EstadoNombre => Estado.ToString();

        /// <summary>
        /// Notas adicionales
        /// </summary>
        public string Notas { get; set; }

        /// <summary>
        /// Fecha de creación de la reservación
        /// </summary>
        public DateTime FechaCreacion { get; set; }
    }
} 