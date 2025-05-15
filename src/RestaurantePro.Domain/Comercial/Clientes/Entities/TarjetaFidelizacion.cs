using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class TarjetaFidelizacion : BaseEntity
    {
        public string Codigo { get; set; }
        public int ClienteId { get; set; }
        public virtual Cliente Cliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaActivacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public EstadoTarjeta Estado { get; set; }
        public int NivelId { get; set; }
        public virtual NivelFidelizacion Nivel { get; set; }
        public int PuntosAcumulados { get; set; }
        public int PuntosDisponibles { get; set; }
        public int PuntosCanjeados { get; set; }
        public virtual ICollection<HistorialPuntos> HistorialPuntos { get; set; } = new List<HistorialPuntos>();
    }
} 