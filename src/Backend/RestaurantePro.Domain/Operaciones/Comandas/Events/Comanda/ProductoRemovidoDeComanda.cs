namespace RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda
{
    /// <summary>
    /// Evento emitido cuando se elimina un producto de una comanda
    /// </summary>
    public class ProductoRemovidoDeComanda : DomainEvent
    {
        /// <summary>
        /// ID de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// ID del producto removido
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Cantidad del producto removida
        /// </summary>
        public int Cantidad { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProductoRemovidoDeComanda(Guid comandaId, Guid productoId, int cantidad)
        {
            ComandaId = comandaId;
            ProductoId = productoId;
            Cantidad = cantidad;
        }
    }
} 