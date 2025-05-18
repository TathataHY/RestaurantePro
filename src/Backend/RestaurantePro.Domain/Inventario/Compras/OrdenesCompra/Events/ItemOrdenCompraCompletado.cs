namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando un item de una orden de compra ha sido completamente recibido
    /// </summary>
    public class ItemOrdenCompraCompletado : DomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del item completado
        /// </summary>
        public Guid ItemId { get; }
        
        /// <summary>
        /// ID del ingrediente
        /// </summary>
        public Guid IngredienteId { get; }
        
        /// <summary>
        /// Cantidad recibida
        /// </summary>
        public decimal CantidadRecibida { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="itemId">ID del item completado</param>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="ingredienteId">ID del ingrediente</param>
        /// <param name="cantidadRecibida">Cantidad recibida</param>
        public ItemOrdenCompraCompletado(Guid itemId, Guid ordenCompraId, Guid ingredienteId, decimal cantidadRecibida)
        {
            ItemId = itemId;
            OrdenCompraId = ordenCompraId;
            IngredienteId = ingredienteId;
            CantidadRecibida = cantidadRecibida;
        }
    }
} 