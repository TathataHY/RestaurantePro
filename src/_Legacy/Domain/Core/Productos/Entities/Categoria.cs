using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa una categoría de productos en el menú
    /// </summary>
    public class Categoria : BaseEntity
    {
        /// <summary>
        /// Nombre de la categoría (único)
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción de la categoría
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// URL de la imagen de la categoría
        /// </summary>
        public string ImagenUrl { get; set; }

        /// <summary>
        /// Orden de visualización de la categoría
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// Fecha de creación de la categoría
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación de la categoría
        /// </summary>
        public DateTime? UltimaModificacion { get; set; }

        /// <summary>
        /// Productos asociados a esta categoría
        /// </summary>
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
} 