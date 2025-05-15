namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa la relación entre un producto y sus ingredientes
    /// </summary>
    public class ProductoIngrediente : BaseEntity
    {
        /// <summary>
        /// ID del producto
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Producto relacionado
        /// </summary>
        public virtual Producto Producto { get; set; }

        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public int IngredienteId { get; set; }

        /// <summary>
        /// Ingrediente relacionado
        /// </summary>
        public virtual Ingrediente Ingrediente { get; set; }

        /// <summary>
        /// Cantidad del ingrediente utilizada en el producto
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Indica si el ingrediente es opcional en el producto
        /// </summary>
        public bool EsOpcional { get; set; }
    }
} 