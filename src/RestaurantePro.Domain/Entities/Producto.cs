using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un producto que se puede ordenar en el restaurante
    /// </summary>
    public class Producto : BaseEntity
    {
        /// <summary>
        /// ID de la categoría a la que pertenece el producto
        /// </summary>
        public int CategoriaId { get; set; }

        /// <summary>
        /// Categoría a la que pertenece el producto
        /// </summary>
        public virtual Categoria Categoria { get; set; }

        /// <summary>
        /// Nombre del producto (único)
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Precio del producto
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// URL de la imagen del producto
        /// </summary>
        public string ImagenUrl { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para ordenar
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>
        /// Tiempo estimado de preparación en minutos
        /// </summary>
        public int TiempoPreparacionMinutos { get; set; }

        /// <summary>
        /// Unidad de medida del producto
        /// </summary>
        public string UnidadMedida { get; set; }

        /// <summary>
        /// Fecha de creación del producto
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última modificación del producto
        /// </summary>
        public DateTime? UltimaModificacion { get; set; }

        /// <summary>
        /// Ingredientes que componen el producto
        /// </summary>
        public virtual ICollection<IngredienteProducto> Ingredientes { get; set; } = new List<IngredienteProducto>();

        /// <summary>
        /// Detalles de comandas que incluyen este producto
        /// </summary>
        public virtual ICollection<ComandaDetalle> ComandaDetalles { get; set; }
    }
} 