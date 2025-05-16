
namespace RestaurantePro.Domain.Core.Productos.Events
{
    /// <summary>
    /// Evento de dominio que representa que se ha desactivado un producto
    /// </summary>
    public class ProductoDesactivado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Identificador único del producto desactivado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Constructor que inicializa un nuevo evento de producto desactivado
        /// </summary>
        public ProductoDesactivado(Guid productoId)
        {
            OccurredOn = DateTime.UtcNow;
            ProductoId = productoId;
        }
    }
}
