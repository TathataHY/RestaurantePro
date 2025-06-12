using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeService _dateTimeService;

        public SoftDeleteInterceptor(
            ICurrentUserService currentUserService,
            IDateTimeService dateTimeService)
        {
            _currentUserService = currentUserService;
            _dateTimeService = dateTimeService;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateEntities(DbContext context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.Eliminado = true;
                    entry.Entity.FechaEliminacion = _dateTimeService.Now;
                    entry.Entity.EliminadoPor = _currentUserService.UserId;

                    // También marcar como modificado el campo Eliminado para asegurar que se actualice
                    entry.Property(nameof(ISoftDelete.Eliminado)).IsModified = true;
                    entry.Property(nameof(ISoftDelete.FechaEliminacion)).IsModified = true;
                    entry.Property(nameof(ISoftDelete.EliminadoPor)).IsModified = true;
                }
            }
        }
    }
} 