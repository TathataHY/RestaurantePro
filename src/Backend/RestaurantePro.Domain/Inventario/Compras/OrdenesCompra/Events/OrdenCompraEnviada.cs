namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando una orden de compra es enviada al proveedor
    /// </summary>
    public class OrdenCompraEnviada : DomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// Fecha de envío
        /// </summary>
        public DateTime FechaEnvio { get; }
        
        /// <summary>
        /// Total de la orden
        /// </summary>
        public decimal Total { get; }
        
                
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="fechaEnvio">Fecha de envío</param>
        /// <param name="total">Total de la orden</param>
        public OrdenCompraEnviada(Guid ordenCompraId, DateTime fechaEnvio, decimal total)
        {
            OrdenCompraId = ordenCompraId;
            FechaEnvio = fechaEnvio;
            Total = total;
        }
    }
} 

