namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualiza el estado de una promoción
    /// </summary>
    public class PromocionEstadoActualizado : DomainEvent
    {
        /// <summary>
        /// Identificador único de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Estado anterior de la promoción
        /// </summary>
        public EstadoPromocion EstadoAnterior { get; }
        
        /// <summary>
        /// Nuevo estado de la promoción
        /// </summary>
        public EstadoPromocion NuevoEstado { get; }
        
        /// <summary>
        /// Fecha en que se realizó el cambio de estado
        /// </summary>
        public DateTime FechaCambio { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionEstadoActualizado
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="estadoAnterior">Estado anterior</param>
        /// <param name="nuevoEstado">Nuevo estado</param>
        /// <param name="fechaCambio">Fecha en que se realizó el cambio</param>
        public PromocionEstadoActualizado(
            Guid promocionId, 
            EstadoPromocion estadoAnterior, 
            EstadoPromocion nuevoEstado, 
            DateTime fechaCambio)
        {
            PromocionId = promocionId;
            EstadoAnterior = estadoAnterior;
            NuevoEstado = nuevoEstado;
            FechaCambio = fechaCambio;
        }
    }
} 