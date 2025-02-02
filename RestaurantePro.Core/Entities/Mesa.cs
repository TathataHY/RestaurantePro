using System;
using System.Collections.Generic;
using RestaurantePro.Core.Entities.Base;
using RestaurantePro.Core.Entities.Aggregates;
using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.Entities
{
    public class Mesa : BaseEntity
    {
        public string Numero { get; set; }
        public int Capacidad { get; set; }
        public EstadoMesa Estado { get; set; }
        public virtual ICollection<Comanda> Comandas { get; set; }
    }
}