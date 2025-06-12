using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTimeService;

        public SoftDeleteInterceptor(
            ICurrentUserService currentUserService,
            IDateTime dateTimeService)
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

            foreach (var entry in context.ChangeTracker.Entries<EntityBase>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    
                    // Establecer la propiedad EstaEliminado a true
                    entry.Property("EstaEliminado").CurrentValue = true;
                    
                    // Actualizar la fecha de modificación
                    entry.Property("FechaActualizacion").CurrentValue = _dateTimeService.Now;
                }
            }
        }
    }
} 