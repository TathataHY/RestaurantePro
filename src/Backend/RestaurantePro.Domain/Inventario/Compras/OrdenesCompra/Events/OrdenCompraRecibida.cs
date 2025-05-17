namespace RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Events
{
    /// <summary>
    /// Evento que se dispara cuando una orden de compra es recibida
    /// </summary>
    public class OrdenCompraRecibida : DomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// Fecha de recepción
        /// </summary>
        public DateTime FechaRecepcion { get; }
        
        /// <summary>
        /// Observaciones de la recepción
        /// </summary>
        public string ObservacionesRecepcion { get; }
        
                
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="fechaRecepcion">Fecha de recepción</param>
        /// <param name="observaciones">Observaciones de la recepción</param>
        public OrdenCompraRecibida(Guid ordenCompraId, DateTime fechaRecepcion, string observaciones)
        {
            OrdenCompraId = ordenCompraId;
            FechaRecepcion = fechaRecepcion;
            ObservacionesRecepcion = observaciones;
        }
    }
} 

