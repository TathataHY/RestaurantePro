namespace RestaurantePro.Domain.Core.Productos.Events.Producto
{
    /// <summary>
    /// Evento de dominio que representa que se ha creado un nuevo producto
    /// </summary>
    public class ProductoCreado : DomainEvent
    {
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
            ProductoId = productoId;
            Nombre = nombre;
            Precio = precio;
        }
    }
}
