using RestaurantePro.Domain.Core.Base;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Manejador de eventos de dominio que invalida automáticamente entradas de caché según reglas configuradas
    /// </summary>
    public class CacheInvalidationEventHandler : IDomainEventHandler<DomainEvent>
    {
        private readonly ICacheService _cacheService;
        private readonly Dictionary<Type, List<string>> _eventToCacheKeyPatterns;
        
        public CacheInvalidationEventHandler(ICacheService cacheService)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _eventToCacheKeyPatterns = new Dictionary<Type, List<string>>();
            ConfigureInvalidationRules();
        }
        
        /// <summary>
        /// Maneja un evento de dominio, invalidando la caché si corresponde según las reglas configuradas
        /// </summary>
        /// <param name="domainEvent">Evento de dominio que ocurrió</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        public async Task Handle(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            if (domainEvent == null)
                return;
                
            var eventType = domainEvent.GetType();
            
            // Invalidar por tipo de evento exacto
            if (_eventToCacheKeyPatterns.TryGetValue(eventType, out var patterns))
            {
                foreach (var pattern in patterns)
                {
                    _cacheService.InvalidatePattern(pattern);
                }
            }
            
            // Buscar por tipos base o interfaces (para eventos que heredan de otros)
            foreach (var kvp in _eventToCacheKeyPatterns)
            {
                if (kvp.Key != eventType && kvp.Key.IsAssignableFrom(eventType))
                {
                    foreach (var pattern in kvp.Value)
                    {
                        _cacheService.InvalidatePattern(pattern);
                    }
                }
            }
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Registra una regla de invalidación para un tipo de evento específico
        /// </summary>
        /// <typeparam name="TEvent">Tipo de evento de dominio</typeparam>
        /// <param name="cacheKeyPattern">Patrón de clave de caché a invalidar</param>
        public void RegisterInvalidationRule<TEvent>(string cacheKeyPattern) where TEvent : DomainEvent
        {
            var eventType = typeof(TEvent);
            
            if (!_eventToCacheKeyPatterns.ContainsKey(eventType))
            {
                _eventToCacheKeyPatterns[eventType] = new List<string>();
            }
            
            _eventToCacheKeyPatterns[eventType].Add(cacheKeyPattern);
        }
        
        /// <summary>
        /// Configura las reglas de invalidación de caché predeterminadas
        /// </summary>
        private void ConfigureInvalidationRules()
        {
            // Para evitar referencias a tipos específicos que pueden no existir,
            // se utiliza la clase base DomainEvent. En una implementación real, 
            // se registrarían tipos específicos de eventos.
            
            // Ejemplo de cómo se registrarían eventos específicos:
            // RegisterInvalidationRule<ProductoCreado>("ProductoCategoriaService_");
            
            // Registrar evento base para invalidación general
            RegisterInvalidationRule<DomainEvent>("ProductoCategoriaService_");
            RegisterInvalidationRule<DomainEvent>("ServicioNotificaciones_");
            RegisterInvalidationRule<DomainEvent>("GeneradorOrdenesCompra_");
            RegisterInvalidationRule<DomainEvent>("VerificadorStock_");
            RegisterInvalidationRule<DomainEvent>("ServicioFidelizacion_");
            RegisterInvalidationRule<DomainEvent>("RecetaService_");
            
            // En una implementación completa, cada evento de dominio específico
            // se registraría con el patrón de caché correspondiente para una
            // invalidación más granular y eficiente.
        }
    }
} 