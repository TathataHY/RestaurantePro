using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class Promocion : BaseEntity
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public TipoPromocion Tipo { get; set; }
        public decimal ValorDescuento { get; set; } // Descuento en valor fijo o porcentaje según el tipo
        public decimal MontoMinimo { get; set; }    // Monto mínimo para aplicar la promoción
        public int PuntosRequeridos { get; set; }   // Puntos necesarios para canjear la promoción
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? MaximoUsos { get; set; }        // Null = usos ilimitados
        public int VecesUsada { get; set; }
        public bool Activa { get; set; }
        
        // Relaciones con categorías o productos específicos a los que aplica la promoción
        public virtual ICollection<Categoria> CategoriasAplicables { get; set; } = new List<Categoria>();
        public virtual ICollection<Producto> ProductosAplicables { get; set; } = new List<Producto>();
        
        // Relación con clientes que la han usado
        public virtual ICollection<Cliente> ClientesQueUsaron { get; set; } = new List<Cliente>();
    }
} 