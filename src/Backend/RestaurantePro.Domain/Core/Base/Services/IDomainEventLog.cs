namespace RestaurantePro.Domain.Core.Base.Services
{
    /// <summary>
    /// Interfaz para el servicio de registro y auditoría de eventos de dominio
    /// </summary>
    public interface IDomainEventLog
    {
        /// <summary>
        /// Registra un evento de dominio para auditoría
        /// </summary>
        /// <param name="evento">Evento a registrar</param>
        /// <param name="resultado">Resultado del procesamiento del evento (éxito, error, etc.)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task LogEvent(DomainEvent evento, string resultado, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los eventos registrados para una entidad específica
        /// </summary>
        /// <param name="entityId">ID de la entidad</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task<IEnumerable<EventoRegistrado>> ObtenerEventosPorEntidad(Guid entityId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene los eventos registrados de un tipo específico
        /// </summary>
        /// <param name="tipoEvento">Nombre del tipo de evento</param>
        /// <param name="fechaDesde">Fecha desde la que buscar</param>
        /// <param name="fechaHasta">Fecha hasta la que buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task<IEnumerable<EventoRegistrado>> ObtenerEventosPorTipo(
            string tipoEvento, 
            DateTime fechaDesde, 
            DateTime fechaHasta, 
            CancellationToken cancellationToken = default);
    }
} 