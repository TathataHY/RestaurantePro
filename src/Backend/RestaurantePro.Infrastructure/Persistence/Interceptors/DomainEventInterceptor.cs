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
            _logger.LogInformation("🔄 [DomainEventInterceptor] SavingChangesAsync - Iniciando recolección de eventos");
            // Recolectar eventos antes de guardar, pero no publicarlos aún
            RecolectarEventosDominio(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            // Publicar eventos sincrónicamente para mantener el contexto activo
            PublicarEventosDominioPendientes().GetAwaiter().GetResult();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("✅ [DomainEventInterceptor] SavedChangesAsync - Iniciando publicación de eventos");
            // Publicar eventos sincrónicamente para mantener el contexto activo
            await PublicarEventosDominioPendientes(cancellationToken);
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void RecolectarEventosDominio(DbContext? context)
        {
            _logger.LogInformation("🔍 [DomainEventInterceptor] INICIO RecolectarEventosDominio");
            
            if (context == null)
            {
                _logger.LogWarning("❌ [DomainEventInterceptor] Context es null");
                return;
            }

            var entidadesConEventos = context.ChangeTracker.Entries<EntityBase>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            _logger.LogInformation("🔍 [DomainEventInterceptor] Encontradas {Count} entidades con eventos", entidadesConEventos.Count);

            if (!entidadesConEventos.Any())
            {
                _logger.LogWarning("⚠️ [DomainEventInterceptor] No hay entidades con eventos");
                return;
            }

            _logger.LogInformation("Recolectando {Count} entidades con eventos de dominio", entidadesConEventos.Count);

            foreach (var entidad in entidadesConEventos)
            {
                var eventos = entidad.DomainEvents.ToList();
                entidad.ClearDomainEvents();
                
                _logger.LogInformation("🔧 [DomainEventInterceptor] Entidad: {EntityType}, ID: {EntityId}, Eventos: {EventCount}", 
                    entidad.GetType().Name, entidad.Id, eventos.Count);
                
                // Agregar eventos a la lista pendiente
                _pendingEvents.AddRange(eventos);
            }
            
            _logger.LogInformation("✅ [DomainEventInterceptor] FIN RecolectarEventosDominio");
        }

        private async Task PublicarEventosDominioPendientes(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🚀 [DomainEventInterceptor] INICIO PublicarEventosDominioPendientes");
            
            if (!_pendingEvents.Any())
            {
                _logger.LogWarning("⚠️ [DomainEventInterceptor] No hay eventos pendientes para publicar");
                return;
            }

            _logger.LogInformation("Publicando {Count} eventos de dominio pendientes", _pendingEvents.Count);

            var eventosParaPublicar = _pendingEvents.ToList();
            _pendingEvents.Clear();

            foreach (var evento in eventosParaPublicar)
            {
                _logger.LogInformation("🚀 [DomainEventInterceptor] Publicando evento: {EventType}", evento.GetType().Name);
                
                _logger.LogInformation("Publicando evento de dominio {EventType}",
                    evento.GetType().Name);
                
                await _domainEventDispatcher.Dispatch(evento, cancellationToken);
            }
            
            _logger.LogInformation("✅ [DomainEventInterceptor] FIN PublicarEventosDominioPendientes");
        }
    }
} 