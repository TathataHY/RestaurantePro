using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un ingrediente utilizado en los productos
    /// </summary>
    public class Ingrediente : BaseEntity
    {
        /// <summary>
        /// Nombre del ingrediente (único)
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción del ingrediente
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Unidad de medida del ingrediente
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Costo unitario del ingrediente
        /// </summary>
        public decimal Costo { get; set; }

        /// <summary>
        /// Stock actual del ingrediente
        /// </summary>
        public decimal StockActual { get; set; }

        /// <summary>
        /// Stock mínimo requerido del ingrediente
        /// </summary>
        public decimal StockMinimo { get; set; }

        /// <summary>
        /// Fecha de creación del ingrediente
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación del ingrediente
        /// </summary>
        public DateTime? UltimaModificacion { get; set; }

        /// <summary>
        /// Productos que utilizan este ingrediente
        /// </summary>
        public virtual ICollection<ProductoIngrediente> ProductoIngredientes { get; set; } = new List<ProductoIngrediente>();

        /// <summary>
        /// Movimientos de inventario de este ingrediente
        /// </summary>
        public virtual ICollection<InventarioMovimiento> InventarioMovimientos { get; set; } = new List<InventarioMovimiento>();

        /// <summary>
        /// Personalizaciones de comandas que utilizan este ingrediente
        /// </summary>
        public virtual ICollection<ComandaDetallePersonalizacion> ComandaDetallePersonalizaciones { get; set; } = new List<ComandaDetallePersonalizacion>();
    }
} 