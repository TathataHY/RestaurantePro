namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se cancela una promoción
    /// </summary>
    public class PromocionCancelada : DomainEvent
    {
        /// <summary>
        /// ID de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Código único de la promoción
        /// </summary>
        public string Codigo { get; }
        
        /// <summary>
        /// Nombre de la promoción
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Motivo de la cancelación
        /// </summary>
        public string Motivo { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionCancelada
        /// </summary>
        public PromocionCancelada(
            Guid promocionId, 
            string codigo, 
            string nombre, 
            string motivo)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
            Motivo = motivo;
        }
    }
} 