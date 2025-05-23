namespace RestaurantePro.Domain.Comercial.Promociones.Events
{
    /// <summary>
    /// Evento de dominio que se dispara cuando se actualizan propiedades de una promoción
    /// </summary>
    public class PromocionActualizada : DomainEvent
    {
        /// <summary>
        /// Identificador único de la promoción
        /// </summary>
        public Guid PromocionId { get; }
        
        /// <summary>
        /// Código de la promoción
        /// </summary>
        public string Codigo { get; }
        
        /// <summary>
        /// Nombre de la promoción
        /// </summary>
        public string Nombre { get; }
        
        /// <summary>
        /// Constructor para el evento PromocionActualizada
        /// </summary>
        /// <param name="promocionId">Identificador de la promoción</param>
        /// <param name="codigo">Código de la promoción</param>
        /// <param name="nombre">Nombre de la promoción</param>
        public PromocionActualizada(Guid promocionId, string codigo, string nombre)
        {
            PromocionId = promocionId;
            Codigo = codigo;
            Nombre = nombre;
        }
    }
} 