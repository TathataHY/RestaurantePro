namespace RestaurantePro.Domain.Core.Productos.Events.ProductoCategoria
{
    /// <summary>
    /// Evento de dominio emitido cuando se activa una categoría de producto
    /// </summary>
    public class ProductoCategoriaActivada : DomainEvent
    {
        /// <summary>
        /// Identificador de la categoría activada
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ProductoCategoriaActivada(Guid id)
        {
            Id = id;
        }
    }
} 