using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Interceptor para auditoría automática de entidades
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;

        public AuditableEntityInterceptor(
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService)
        {
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Se ejecuta antes de guardar cambios
        /// </summary>
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
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
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Actualiza las propiedades de auditoría de las entidades
        /// </summary>
        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreadoPor = _currentUserService.UserId;
                    entry.Entity.FechaCreacion = _dateTimeService.Now;
                }

                if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Entity.ModificadoPor = _currentUserService.UserId;
                    entry.Entity.FechaModificacion = _dateTimeService.Now;
                }
            }
        }
    }

    /// <summary>
    /// Extensiones para el ChangeTracker
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Verifica si una entidad tiene cambios en entidades propias
        /// </summary>
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r => 
                r.TargetEntry != null && 
                r.TargetEntry.Metadata.IsOwned() && 
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
} 