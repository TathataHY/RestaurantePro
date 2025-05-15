using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa una mesa física del restaurante
    /// </summary>
    public class Mesa : BaseEntity
    {
        /// <summary>
        /// Número de la mesa (único)
        /// </summary>
        public int Numero { get; set; }

        /// <summary>
        /// Capacidad de personas en la mesa
        /// </summary>
        public int Capacidad { get; set; }

        /// <summary>
        /// Ubicación de la mesa en el restaurante
        /// </summary>
        public string Ubicacion { get; set; }

        /// <summary>
        /// Indica si la mesa está activa para su uso
        /// </summary>
        public bool Activa { get; set; }

        /// <summary>
        /// Estado actual de la mesa
        /// </summary>
        public EstadoMesa Estado { get; set; }

        /// <summary>
        /// Código QR para identificar la mesa
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación
        /// </summary>
        public DateTime? UltimaModificacion { get; set; }

        /// <summary>
        /// Comandas asociadas a esta mesa
        /// </summary>
        public virtual ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();

        /// <summary>
        /// Reservaciones asociadas a esta mesa
        /// </summary>
        public virtual ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
    }
} 