namespace RestaurantePro.Domain.Core.Base.Events
{
    /// <summary>
    /// Representa un evento de dominio registrado para auditoría
    /// </summary>
    public class EventoRegistrado
    {
        /// <summary>
        /// ID único del registro
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// ID de la entidad relacionada con el evento
        /// </summary>
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// Tipo de evento (nombre completo de la clase)
        /// </summary>
        public string TipoEvento { get; set; } = string.Empty;
        
        /// <summary>
        /// Datos del evento serializados
        /// </summary>
        public string DatosEvento { get; set; } = string.Empty;
        
        /// <summary>
        /// Fecha y hora en que ocurrió el evento
        /// </summary>
        public DateTime FechaOcurrencia { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se registró el evento
        /// </summary>
        public DateTime FechaRegistro { get; set; }
        
        /// <summary>
        /// Resultado del procesamiento del evento
        /// </summary>
        public string Resultado { get; set; } = string.Empty;
    }
} 