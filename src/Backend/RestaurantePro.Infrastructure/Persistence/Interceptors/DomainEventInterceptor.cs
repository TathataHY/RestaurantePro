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
            Console.WriteLine("[DomainEventInterceptor] 🔍 INICIO PublicarEventosDominio");
            
            if (context == null)
            {
                Console.WriteLine("[DomainEventInterceptor] ❌ Context es null");
                return;
            }

            var entidadesConEventos = context.ChangeTracker.Entries<EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            Console.WriteLine($"[DomainEventInterceptor] 🔍 Encontradas {entidadesConEventos.Count} entidades con eventos");

            if (!entidadesConEventos.Any())
            {
                Console.WriteLine("[DomainEventInterceptor] ⚠️ No hay entidades con eventos");
                return;
            }

            _logger.LogInformation("Encontradas {Count} entidades con eventos de dominio para publicar", entidadesConEventos.Count);

            foreach (var entidad in entidadesConEventos)
            {
                var eventos = entidad.DomainEvents.ToList();
                entidad.ClearDomainEvents();
                
                Console.WriteLine($"[DomainEventInterceptor] 🔧 Entidad: {entidad.GetType().Name}, ID: {entidad.Id}, Eventos: {eventos.Count}");
                
                foreach (var evento in eventos)
                {
                    Console.WriteLine($"[DomainEventInterceptor] 🚀 Publicando evento: {evento.GetType().Name}");
                    
                    _logger.LogInformation("Publicando evento de dominio {EventType} para la entidad {EntityType} con ID {EntityId}",
                        evento.GetType().Name, entidad.GetType().Name, entidad.Id);
                    
                    await _domainEventDispatcher.Dispatch(evento, cancellationToken);
                }
            }
            
            Console.WriteLine("[DomainEventInterceptor] ✅ FIN PublicarEventosDominio");
        }
    }
} 