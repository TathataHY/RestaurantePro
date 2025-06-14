using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor para borrado lógico de entidades
    /// </summary>
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SoftDeleteInterceptor> _logger;

        public SoftDeleteInterceptor(
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            ILogger<SoftDeleteInterceptor> logger)
        {
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Se ejecuta antes de guardar cambios
        /// </summary>
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ProcesarEntidadesEliminadas(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// Se ejecuta antes de guardar cambios de forma asíncrona
        /// </summary>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ProcesarEntidadesEliminadas(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Actualiza las propiedades de borrado lógico de las entidades
        /// </summary>
        private void ProcesarEntidadesEliminadas(DbContext? context)
        {
            if (context == null) return;

            var entidadesEliminadas = context.ChangeTracker.Entries<EntityBase>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            if (!entidadesEliminadas.Any()) return;

            _logger.LogInformation("Procesando {Count} entidades para borrado lógico", entidadesEliminadas.Count);

            foreach (var entry in entidadesEliminadas)
            {
                // Cambiar el estado de la entidad a modificado
                entry.State = EntityState.Modified;
                
                // Marcar la entidad como eliminada lógicamente
                entry.Entity.MarkAsDeleted();
                
                _logger.LogInformation("Aplicando borrado lógico a entidad {EntityType} con ID {EntityId}", 
                    entry.Entity.GetType().Name, entry.Entity.Id);
            }
        }
    }
} 