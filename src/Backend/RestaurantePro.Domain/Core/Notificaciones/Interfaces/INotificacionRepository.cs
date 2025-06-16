using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;

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
            
        /// <summary>
        /// Elimina las notificaciones anteriores a una fecha determinada
        /// </summary>
        /// <param name="fecha">Fecha límite para eliminar notificaciones anteriores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de notificaciones eliminadas</returns>
        Task<int> EliminarAnterioresAFechaAsync(DateTime fecha, CancellationToken cancellationToken = default);
    }
} 