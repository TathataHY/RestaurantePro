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
        /// Fecha en que ocurrió el evento
        /// </summary>
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ProductoAgregadoAComanda(Guid comandaId, Guid productoId, int cantidad)
        {
            ComandaId = comandaId;
            ProductoId = productoId;
            Cantidad = cantidad;
                    }
    }
}


