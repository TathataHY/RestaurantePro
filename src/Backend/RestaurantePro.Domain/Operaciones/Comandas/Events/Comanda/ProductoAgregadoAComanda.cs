namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se agrega un producto a una comanda
    /// </summary>
    public class ProductoAgregadoAComanda : DomainEvent
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// ID del producto agregado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Cantidad del producto agregada
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Precio unitario del producto
        /// </summary>
        public decimal PrecioUnitario { get; }

        /// <summary>
        /// Fecha en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ProductoAgregadoAComanda(Guid comandaId, Guid productoId, int cantidad, decimal precioUnitario)
        {
            ComandaId = comandaId;
            ProductoId = productoId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }
        
        /// <summary>
        /// Constructor para compatibilidad con código existente
        /// </summary>
        public ProductoAgregadoAComanda(Guid comandaId, Guid productoId, int cantidad)
            : this(comandaId, productoId, cantidad, 0)
        {
        }
    }
}


