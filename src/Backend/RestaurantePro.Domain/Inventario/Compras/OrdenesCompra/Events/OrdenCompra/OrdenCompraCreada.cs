namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.OrdenCompra
{
    /// <summary>
    /// Evento que se dispara cuando se crea una nueva orden de compra
    /// </summary>
    public class OrdenCompraCreada : DomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// ID del proveedor asociado
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Fecha de emisión de la orden
        /// </summary>
        public DateTime FechaEmision { get; }
        
        /// <summary>
        /// Fecha estimada de entrega (opcional)
        /// </summary>
        public DateTime? FechaEntregaEstimada { get; }
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        /// <param name="fechaEntregaEstimada">Fecha estimada de entrega (opcional)</param>
        public OrdenCompraCreada(Guid ordenCompraId, Guid proveedorId, DateTime fechaEmision, DateTime? fechaEntregaEstimada = null)
        {
            OrdenCompraId = ordenCompraId;
            ProveedorId = proveedorId;
            FechaEmision = fechaEmision;
            FechaEntregaEstimada = fechaEntregaEstimada;
        }
    }
} 
