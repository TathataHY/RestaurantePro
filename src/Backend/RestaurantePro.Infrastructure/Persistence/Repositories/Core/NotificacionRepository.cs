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
        private new readonly RestauranteProDbContext _dbContext;

        public NotificacionRepository(RestauranteProDbContext dbContext, ILogger<NotificacionRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
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
                .Where(n => n.DestinatarioId == destinatarioId && !n.EstaLeida)
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync(cancellationToken);
        }
    }
} 