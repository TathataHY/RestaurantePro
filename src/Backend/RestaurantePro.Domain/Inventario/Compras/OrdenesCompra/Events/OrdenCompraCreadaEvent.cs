namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando se crea una orden de compra
    /// </summary>
    public class OrdenCompraCreadaEvent : IDomainEvent
    {
        /// <summary>
        /// Momento en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;
        
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del proveedor
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Fecha de la orden
        /// </summary>
        public DateTime FechaEmision { get; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEmision">Fecha de emisión de la orden</param>
        public OrdenCompraCreadaEvent(Guid ordenCompraId, Guid proveedorId, DateTime fechaEmision)
        {
            OrdenCompraId = ordenCompraId;
            ProveedorId = proveedorId;
            FechaEmision = fechaEmision;
        }
    }
} 