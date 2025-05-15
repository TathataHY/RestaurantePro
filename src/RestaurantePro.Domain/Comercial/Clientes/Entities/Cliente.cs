using RestaurantePro.Domain.Common;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class Cliente : BaseEntity
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaVisita { get; set; }
        public int TotalVisitas { get; set; }
        public decimal TotalGastado { get; set; }
        public int PuntosAcumulados { get; set; }
        public int PuntosRedimidos { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
        
        // Relaciones
        public virtual ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
        public virtual ICollection<Reservacion> Reservaciones { get; set; } = new List<Reservacion>();
        public virtual ICollection<Promocion> PromocionesUsadas { get; set; } = new List<Promocion>();
    }
} 