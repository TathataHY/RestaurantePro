using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    public class DomainEventInterceptor : SaveChangesInterceptor
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<DomainEventInterceptor> _logger;

        public DomainEventInterceptor(
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<DomainEventInterceptor> logger)
        {
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            PublicarEventosDominio(eventData.Context).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await PublicarEventosDominio(eventData.Context, cancellationToken);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task PublicarEventosDominio(DbContext? context, CancellationToken cancellationToken = default)
        {
            if (context == null) return;

            var entidadesConEventos = context.ChangeTracker.Entries<EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            if (!entidadesConEventos.Any()) return;

            _logger.LogInformation("Encontradas {Count} entidades con eventos de dominio para publicar", entidadesConEventos.Count);

            foreach (var entidad in entidadesConEventos)
            {
                var eventos = entidad.DomainEvents.ToList();
                entidad.ClearDomainEvents();
                
                foreach (var evento in eventos)
                {
                    _logger.LogInformation("Publicando evento de dominio {EventType} para la entidad {EntityType} con ID {EntityId}",
                        evento.GetType().Name, entidad.GetType().Name, entidad.Id);
                    
                    await _domainEventDispatcher.Dispatch(evento, cancellationToken);
                }
            }
        }
    }
} 