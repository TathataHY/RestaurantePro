using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Interceptors
{
    public class DomainEventInterceptor : SaveChangesInterceptor
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILogger<DomainEventInterceptor> _logger;
        private readonly List<DomainEvent> _pendingEvents = new();

        public DomainEventInterceptor(
            IDomainEventDispatcher domainEventDispatcher,
            ILogger<DomainEventInterceptor> logger)
        {
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            // Recolectar eventos antes de guardar, pero no publicarlos aún
            RecolectarEventosDominio(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            // Recolectar eventos antes de guardar, pero no publicarlos aún
            RecolectarEventosDominio(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            // Publicar eventos en background para no bloquear la transacción
            _ = Task.Run(() => PublicarEventosDominioPendientes());
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            // Publicar eventos en background para no bloquear la transacción
            _ = Task.Run(() => PublicarEventosDominioPendientes(cancellationToken), cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void RecolectarEventosDominio(DbContext? context)
        {
            Console.WriteLine("[DomainEventInterceptor] 🔍 INICIO RecolectarEventosDominio");
            
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

            _logger.LogInformation("Recolectando {Count} entidades con eventos de dominio", entidadesConEventos.Count);

            foreach (var entidad in entidadesConEventos)
            {
                var eventos = entidad.DomainEvents.ToList();
                entidad.ClearDomainEvents();
                
                Console.WriteLine($"[DomainEventInterceptor] 🔧 Entidad: {entidad.GetType().Name}, ID: {entidad.Id}, Eventos: {eventos.Count}");
                
                // Agregar eventos a la lista pendiente
                _pendingEvents.AddRange(eventos);
            }
            
            Console.WriteLine("[DomainEventInterceptor] ✅ FIN RecolectarEventosDominio");
        }

        private async Task PublicarEventosDominioPendientes(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[DomainEventInterceptor] 🚀 INICIO PublicarEventosDominioPendientes");
            
            if (!_pendingEvents.Any())
            {
                Console.WriteLine("[DomainEventInterceptor] ⚠️ No hay eventos pendientes para publicar");
                return;
            }

            _logger.LogInformation("Publicando {Count} eventos de dominio pendientes", _pendingEvents.Count);

            var eventosParaPublicar = _pendingEvents.ToList();
            _pendingEvents.Clear();

            foreach (var evento in eventosParaPublicar)
            {
                Console.WriteLine($"[DomainEventInterceptor] 🚀 Publicando evento: {evento.GetType().Name}");
                
                _logger.LogInformation("Publicando evento de dominio {EventType}",
                    evento.GetType().Name);
                
                await _domainEventDispatcher.Dispatch(evento, cancellationToken);
            }
            
            Console.WriteLine("[DomainEventInterceptor] ✅ FIN PublicarEventosDominioPendientes");
        }
    }
} 