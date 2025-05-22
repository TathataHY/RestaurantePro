namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.ItemOrdenCompra
{
    /// <summary>
    /// Evento que se dispara cuando se elimina un item de una orden de compra
    /// </summary>
    public class ItemOrdenCompraEliminado : DomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del item eliminado
        /// </summary>
        public Guid ItemId { get; }
        
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Nuevo total de la orden de compra
        /// </summary>
        public decimal NuevoTotal { get; }
        
                
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="itemId">ID del item eliminado</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="nuevoTotal">Nuevo total de la orden</param>
        public ItemOrdenCompraEliminado(Guid ordenCompraId, Guid itemId, Guid ingredienteId, decimal nuevoTotal)
        {
            OrdenCompraId = ordenCompraId;
            ItemId = itemId;
            IngredienteId = ingredienteId;
            NuevoTotal = nuevoTotal;
        }
    }
} 

