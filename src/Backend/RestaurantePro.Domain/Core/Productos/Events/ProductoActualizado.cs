
namespace RestaurantePro.Domain.Core.Productos.Events
{
    /// <summary>
    /// Evento de dominio que representa que se ha actualizado un producto
    /// </summary>
    public class ProductoActualizado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }
        
        /// <summary>
        /// Identificador único del producto actualizado
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// Nombre actualizado del producto
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Descripción actualizada del producto
        /// </summary>
        public string Descripcion { get; }
        
        /// <summary>
        /// Precio actualizado del producto
        /// </summary>
        public decimal Precio { get; }
        
        /// <summary>
        /// Constructor que inicializa un nuevo evento de producto actualizado
        /// </summary>
        public ProductoActualizado(Guid productoId, string nombre, string descripcion, decimal precio)
        {
            OccurredOn = DateTime.UtcNow;
            ProductoId = productoId;
            Nombre = nombre;
            Descripcion = descripcion;
            Precio = precio;
        }
    }
} 