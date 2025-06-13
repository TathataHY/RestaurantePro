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
    /// Interceptor para borrado lógico de entidades
    /// </summary>
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;

        public SoftDeleteInterceptor(
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
        /// Actualiza las propiedades de borrado lógico de las entidades
        /// </summary>
        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.Activo = false;
                    entry.Entity.FechaEliminacion = _dateTimeService.Now;
                    entry.Entity.EliminadoPor = _currentUserService.UserId;
                }
            }
        }
    }
} 