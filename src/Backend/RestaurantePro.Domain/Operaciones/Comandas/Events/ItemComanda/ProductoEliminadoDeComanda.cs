namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se elimina un producto de una comanda
    /// </summary>
    public class ProductoEliminadoDeComanda : DomainEvent
    {
        /// <summary>
        /// Identificador único de la comanda
        /// </summary>
        public Guid ComandaId { get; }

        /// <summary>
        /// Identificador único del ítem eliminado
        /// </summary>
        public Guid ItemId { get; }

        /// <summary>
        /// Identificador único del producto eliminado
        /// </summary>
        public Guid ProductoId { get; }

        /// <summary>
        /// Nombre del producto eliminado
        /// </summary>
        public string NombreProducto { get; }

        /// <summary>
        /// Cantidad de producto eliminada
        /// </summary>
        public int Cantidad { get; }
        
        /// <summary>
        /// Motivo de la eliminación
        /// </summary>
        public string Motivo { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ProductoEliminadoDeComanda(
            Guid comandaId,
            Guid itemId,
            Guid productoId,
            string nombreProducto,
            int cantidad,
            string motivo)
        {
            ComandaId = comandaId;
            ItemId = itemId;
            ProductoId = productoId;
            NombreProducto = nombreProducto;
            Cantidad = cantidad;
            Motivo = motivo;
        }
    }
} 