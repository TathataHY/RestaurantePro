using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    /// <summary>
    /// Repositorio para la entidad Notificacion
    /// </summary>
    public class NotificacionRepository : Repository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(DbContext dbContext, ILogger<NotificacionRepository> logger)
            : base(dbContext, logger)
        {
        }

        /// <summary>
        /// Obtiene las notificaciones por destinatario
        /// </summary>
        public async Task<IEnumerable<Notificacion>> ObtenerPorDestinatarioAsync(
            Guid destinatarioId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.DestinatarioId == destinatarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
            
        /// <summary>
        /// Obtiene las notificaciones por tipo y destinatario
        /// </summary>
        public async Task<IEnumerable<Notificacion>> ObtenerPorTipoYDestinatarioAsync(
            TipoNotificacion tipo, 
            Guid destinatarioId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.Tipo == tipo && n.DestinatarioId == destinatarioId)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
            
        /// <summary>
        /// Obtiene las notificaciones no leídas por destinatario
        /// </summary>
        public async Task<IEnumerable<Notificacion>> ObtenerNoLeidasPorDestinatarioAsync(
            Guid destinatarioId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(n => n.DestinatarioId == destinatarioId && n.FechaLectura == null)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Elimina las notificaciones anteriores a una fecha determinada
        /// </summary>
        /// <param name="fecha">Fecha límite para eliminar notificaciones anteriores</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de notificaciones eliminadas</returns>
        public async Task<int> EliminarAnterioresAFechaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var notificacionesAntiguas = await _dbSet
                .Where(n => n.FechaCreacion < fecha)
                .ToListAsync(cancellationToken);
            
            if (notificacionesAntiguas.Any())
            {
                _dbSet.RemoveRange(notificacionesAntiguas);
                _logger.LogInformation("Eliminando {Count} notificaciones antiguas anteriores a {Fecha}", notificacionesAntiguas.Count, fecha);
            }
            
            return notificacionesAntiguas.Count;
        }

        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesNoLeidasAsync(string usuarioId)
        {
            var destinatarioGuid = Guid.Parse(usuarioId);
            return await _dbSet.Where(n => n.DestinatarioId == destinatarioGuid && !n.EstaLeida).ToListAsync();
        }
    }
} 