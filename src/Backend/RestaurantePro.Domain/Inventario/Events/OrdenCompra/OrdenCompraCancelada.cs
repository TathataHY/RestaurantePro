namespace RestaurantePro.Domain.Inventario.Events
{
    /// <summary>
    /// Evento que se dispara cuando una orden de compra es cancelada
    /// </summary>
    public class OrdenCompraCancelada : Core.Base.Interfaces.IDomainEvent
    {
        /// <summary>
        /// ID de la orden de compra
        /// </summary>
        public Guid OrdenCompraId { get; }
        
        /// <summary>
        /// Fecha de cancelación
        /// </summary>
        public DateTime FechaCancelacion { get; }
        
        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string MotivoCancelacion { get; }
        
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime OccurredOn { get; } = DateTime.Now;
        
        /// <summary>
        /// Constructor del evento
        /// </summary>
        /// <param name="ordenCompraId">ID de la orden de compra</param>
        /// <param name="fechaCancelacion">Fecha de cancelación</param>
        /// <param name="motivoCancelacion">Motivo de la cancelación</param>
        public OrdenCompraCancelada(Guid ordenCompraId, DateTime fechaCancelacion, string motivoCancelacion)
        {
            OrdenCompraId = ordenCompraId;
            FechaCancelacion = fechaCancelacion;
            MotivoCancelacion = motivoCancelacion;
        }
    }
} 