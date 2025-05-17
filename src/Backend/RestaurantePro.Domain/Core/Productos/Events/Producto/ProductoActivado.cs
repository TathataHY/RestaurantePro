namespace RestaurantePro.Domain.Core.Productos.Events.Producto
{
    /// <summary>
    /// Evento de dominio que representa que se ha activado un producto
    /// </summary>
    public class ProductoActivado : DomainEvent
    {
        
        /// <summary>
        /// Identificador único del producto activado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Constructor que inicializa un nuevo evento de producto activado
        /// </summary>
        public ProductoActivado(Guid productoId)
        {
                        ProductoId = productoId;
        }
    }
}

