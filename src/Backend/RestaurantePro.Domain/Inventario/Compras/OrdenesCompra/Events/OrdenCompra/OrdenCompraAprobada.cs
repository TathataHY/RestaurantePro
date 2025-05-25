namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events.OrdenCompra
{
    /// <summary>
    /// Evento de dominio que se emite cuando una orden de compra es aprobada
    /// </summary>
    public class OrdenCompraAprobada : DomainEvent
    {
        /// <summary>
        /// Identificador de la orden de compra aprobada
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// Identificador del proveedor asociado a la orden
        /// </summary>
        public Guid ProveedorId { get; }
        
        /// <summary>
        /// Fecha en que se aprobó la orden de compra
        /// </summary>
        public DateTime FechaAprobacion { get; }
        
        /// <summary>
        /// Monto total de la orden de compra
        /// </summary>
        public decimal Total { get; }
        
        /// <summary>
        /// Constructor con los datos del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="fechaAprobacion">Fecha de aprobación</param>
        /// <param name="total">Monto total de la orden</param>
        public OrdenCompraAprobada(
            Guid ordenCompraId, 
            Guid proveedorId,
            DateTime fechaAprobacion,
            decimal total) 
            : base(ordenCompraId)
        {
            OrdenCompraId = ordenCompraId;
            ProveedorId = proveedorId;
            FechaAprobacion = fechaAprobacion;
            Total = total;
        }
    }
} 