namespace RestaurantePro.Domain.Core.Productos.Events.Producto
{
    /// <summary>
    /// Evento de dominio que representa que se ha actualizado la imagen de un producto
    /// </summary>
    public class ImagenProductoActualizada : DomainEvent
    {
        /// <summary>
        /// Identificador único del producto
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Nueva URL de la imagen del producto
        /// </summary>
        public string? ImagenUrl { get; }

        /// <summary>
        /// Constructor que inicializa un nuevo evento de imagen de producto actualizada
        /// </summary>
        public ImagenProductoActualizada(Guid productoId, string? imagenUrl)
        {
            ProductoId = productoId;
            ImagenUrl = imagenUrl;
        }
    }
}
