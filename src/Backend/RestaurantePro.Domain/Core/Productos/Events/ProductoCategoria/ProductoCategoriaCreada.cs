namespace RestaurantePro.Domain.Core.Productos.Events.ProductoCategoria
{
    /// <summary>
    /// Evento de dominio emitido cuando se crea una nueva categoría de producto
    /// </summary>
    public class ProductoCategoriaCreada : DomainEvent
    {
        /// <summary>
        /// Identificador de la categoría creada
        /// </summary>
        public Guid Id { get; }
        
        /// <summary>
        /// Nombre de la categoría
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ProductoCategoriaCreada(Guid id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }
    }
} 