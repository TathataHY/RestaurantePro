using RestaurantePro.Core.Entities.Base;
using RestaurantePro.Core.Identity;
using RestaurantePro.Core.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Core.Entities
{
    public class Comanda : BaseEntity
    {
        public DateTime FechaHora { get; set; }
        public int MesaId { get; set; }
        public virtual Mesa Mesa { get; set; }
        public string MeseroId { get; set; }
        public virtual ApplicationUser Mesero { get; set; }
        public decimal Total { get; set; }
        public EstadoComanda Estado { get; set; }
        public string Observaciones { get; set; }
        public virtual ICollection<ComandaDetalle> Detalles { get; set; } = new List<ComandaDetalle>();
    }
} 