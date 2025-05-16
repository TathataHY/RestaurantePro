namespace RestaurantePro.Domain.Core.Productos.Events
{
    /// <summary>
    /// Evento de dominio que representa que se ha creado un nuevo producto
    /// </summary>
    public class ProductoCreado : IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Identificador único del producto creado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Nombre del producto creado
        /// </summary>
        public string Nombre { get; }

        /// <summary>
        /// Precio del producto creado
        /// </summary>
        public decimal Precio { get; }

        /// <summary>
        /// Constructor que inicializa un nuevo evento de producto creado
        /// </summary>
        public ProductoCreado(Guid productoId, string nombre, decimal precio)
        {
            OccurredOn = DateTime.UtcNow;
            ProductoId = productoId;
            Nombre = nombre;
            Precio = precio;
        }
    }
}
