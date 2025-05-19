namespace RestaurantePro.Domain.Core.Productos.Events.Producto
{
    /// <summary>
    /// Evento de dominio emitido cuando se actualiza la categoría de un producto
    /// </summary>
    public class ProductoCambioCategoria : DomainEvent
    {
        /// <summary>
        /// Identificador del producto
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Identificador de la nueva categoría
        /// </summary>
        public Guid CategoriaId { get; }
        
        /// <summary>
        /// Nombre de la nueva categoría
        /// </summary>
        public string CategoriaNombre { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ProductoCambioCategoria(Guid productoId, Guid categoriaId, string categoriaNombre)
        {
            ProductoId = productoId;
            CategoriaId = categoriaId;
            CategoriaNombre = categoriaNombre;
        }
    }
} 