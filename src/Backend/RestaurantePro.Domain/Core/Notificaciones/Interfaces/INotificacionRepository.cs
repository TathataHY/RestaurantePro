namespace RestaurantePro.Domain.Core.Notificaciones.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de notificaciones
    /// </summary>
    public interface INotificacionRepository : IRepository<Notificacion>
    {
        /// <summary>
        /// Obtiene las notificaciones por destinatario
        /// </summary>
        Task<IEnumerable<Notificacion>> ObtenerPorDestinatarioAsync(
            Guid destinatarioId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Obtiene las notificaciones por tipo y destinatario
        /// </summary>
        Task<IEnumerable<Notificacion>> ObtenerPorTipoYDestinatarioAsync(
            TipoNotificacion tipo, 
            Guid destinatarioId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Obtiene las notificaciones no leídas por destinatario
        /// </summary>
        Task<IEnumerable<Notificacion>> ObtenerNoLeidasPorDestinatarioAsync(
            Guid destinatarioId, 
            CancellationToken cancellationToken = default);
    }
} 