namespace RestaurantePro.Domain.Core.Productos.Events.ProductoCategoria
{
    /// <summary>
    /// Evento de dominio emitido cuando se desactiva una categoría de producto
    /// </summary>
    public class ProductoCategoriaDesactivada : DomainEvent
    {
        /// <summary>
        /// Identificador de la categoría desactivada
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ProductoCategoriaDesactivada(Guid id)
        {
            Id = id;
        }
    }
} 