namespace RestaurantePro.Domain.Core.Productos.Events.Producto
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza la popularidad de un producto
    /// </summary>
    public class PopularidadProductoActualizada : DomainEvent
    {
        /// <summary>
        /// ID del producto cuya popularidad fue actualizada
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Valor anterior de popularidad
        /// </summary>
        public int PopularidadAnterior { get; }
        
        /// <summary>
        /// Nuevo valor de popularidad
        /// </summary>
        public int NuevaPopularidad { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="popularidadAnterior">Valor anterior de popularidad</param>
        /// <param name="nuevaPopularidad">Nuevo valor de popularidad</param>
        public PopularidadProductoActualizada(Guid productoId, int popularidadAnterior, int nuevaPopularidad)
        {
            ProductoId = productoId;
            PopularidadAnterior = popularidadAnterior;
            NuevaPopularidad = nuevaPopularidad;
        }
    }
} 