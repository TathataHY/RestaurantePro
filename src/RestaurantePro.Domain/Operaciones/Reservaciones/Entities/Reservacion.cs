using RestaurantePro.Domain.Enums;
using System;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa una reservación de mesa
    /// </summary>
    public class Reservacion : BaseEntity
    {
        /// <summary>
        /// ID de la mesa reservada
        /// </summary>
        public int MesaId { get; set; }

        /// <summary>
        /// Mesa reservada
        /// </summary>
        public virtual Mesa Mesa { get; set; }

        /// <summary>
        /// Nombre del cliente que hizo la reservación
        /// </summary>
        public string NombreCliente { get; set; }

        /// <summary>
        /// Teléfono de contacto del cliente
        /// </summary>
        public string Telefono { get; set; }

        /// <summary>
        /// Email de contacto del cliente
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Fecha y hora de la reservación
        /// </summary>
        public DateTime FechaReservacion { get; set; }

        /// <summary>
        /// Duración estimada de la reservación en minutos
        /// </summary>
        public int DuracionEstimada { get; set; }

        /// <summary>
        /// Número de personas para la reservación
        /// </summary>
        public int NumeroPersonas { get; set; }

        /// <summary>
        /// Estado actual de la reservación
        /// </summary>
        public EstadoReservacion Estado { get; set; }

        /// <summary>
        /// Notas adicionales sobre la reservación
        /// </summary>
        public string Notas { get; set; }
    }
} 