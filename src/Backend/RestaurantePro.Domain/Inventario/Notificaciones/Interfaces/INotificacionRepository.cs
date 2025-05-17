namespace RestaurantePro.Domain.Inventario.Notificaciones.Interfaces
{
    using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
    using RestaurantePro.Domain.Inventario.Notificaciones.Entities;
    using RestaurantePro.Domain.Inventario.Notificaciones.Enums;

    /// <summary>
    /// Interfaz para el repositorio de notificaciones
    /// </summary>
    public interface INotificacionRepository : IRepository<Notificacion>
    {
        /// <summary>
        /// Obtiene una notificación por su ID
        /// </summary>
        /// <param name="id">ID de la notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>La notificación correspondiente, o null si no existe</returns>
        Task<Notificacion> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las notificaciones pendientes para un destinatario
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de notificaciones pendientes</returns>
        Task<IEnumerable<Notificacion>> ObtenerPendientesPorDestinatarioAsync(Guid destinatarioId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las notificaciones para un destinatario
        /// </summary>
        /// <param name="destinatarioId">ID del destinatario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de notificaciones</returns>
        Task<IEnumerable<Notificacion>> ObtenerPorDestinatarioAsync(Guid destinatarioId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene todas las notificaciones de un tipo específico
        /// </summary>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de notificaciones</returns>
        Task<IEnumerable<Notificacion>> ObtenerPorTipoAsync(TipoNotificacion tipo, CancellationToken cancellationToken = default);
    }
} 