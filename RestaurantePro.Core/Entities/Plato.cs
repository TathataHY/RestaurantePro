using System;
using System.Collections.Generic;
using RestaurantePro.Core.Entities.Base;
using RestaurantePro.Core.Entities.Aggregates;

namespace RestaurantePro.Core.Entities
{
    public class Plato : BaseEntity
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public bool Disponible { get; set; }
        public virtual ICollection<ComandaDetalle> ComandaDetalles { get; set; }
    }
} 