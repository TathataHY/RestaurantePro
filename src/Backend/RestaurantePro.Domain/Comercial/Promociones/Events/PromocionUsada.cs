namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se utiliza una promoción
    /// </summary>
    public class PromocionUsada : DomainEvent
    {
        /// <summary>
        /// Identificador único de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Identificador del cliente que usó la promoción
        /// </summary>
        public Guid ClienteId { get; }
        
        /// <summary>
        /// Identificador de la comanda donde se aplicó la promoción
        /// </summary>
        public Guid ComandaId { get; }
        
        /// <summary>
        /// Monto del descuento aplicado
        /// </summary>
        public decimal MontoAplicado { get; }
        
        /// <summary>
        /// Fecha en que se usó la promoción
        /// </summary>
        public DateTime FechaUso { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionUsada
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="clienteId">Identificador del cliente</param>
        /// <param name="comandaId">Identificador de la comanda</param>
        /// <param name="montoAplicado">Monto del descuento aplicado</param>
        /// <param name="fechaUso">Fecha en que se usó la promoción</param>
        public PromocionUsada(
            Guid promocionId, 
            Guid clienteId, 
            Guid comandaId, 
            decimal montoAplicado, 
            DateTime fechaUso)
        {
            PromocionId = promocionId;
            ClienteId = clienteId;
            ComandaId = comandaId;
            MontoAplicado = montoAplicado;
            FechaUso = fechaUso;
        }
    }
} 