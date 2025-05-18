namespace RestaurantePro.Domain.Operaciones.Comandas.Events.ItemComanda
{
    /// <summary>
    /// Evento que se dispara cuando un ítem de comanda es entregado al cliente
    /// Este evento desencadena la actualización del inventario
    /// </summary>
    public class ItemComandaEntregado : DomainEvent
    {
        /// <summary>
        /// ID del ítem de comanda
        /// </summary>
        public Guid ItemComandaId { get; }
        
        /// <summary>
        /// ID del producto asociado al ítem
        /// </summary>
        public Guid ProductoId { get; }
        
        /// <summary>
        /// ID de la comanda a la que pertenece el ítem
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Cantidad del producto en este ítem
        /// </summary>
        public int Cantidad { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        public ItemComandaEntregado(Guid itemComandaId, Guid productoId, Guid comandaId, int cantidad)
        {
            ItemComandaId = itemComandaId;
            ProductoId = productoId;
            ComandaId = comandaId;
            Cantidad = cantidad;
        }
    }
} 