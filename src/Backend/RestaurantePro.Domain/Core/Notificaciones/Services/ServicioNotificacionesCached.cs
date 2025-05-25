using RestaurantePro.Domain.Core.Notificaciones.Entities;
using RestaurantePro.Domain.Core.Notificaciones.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.Notificaciones.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones con soporte de caché para mejorar el rendimiento
    /// </summary>
    public class ServicioNotificacionesCached : IServicioNotificacionesCached
    {
        private readonly IServicioNotificaciones _servicioOriginal;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "ServicioNotificaciones_";
        private const int CacheDurationMinutes = 30; // Caché de 30 minutos para notificaciones
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ServicioNotificacionesCached(
            IServicioNotificaciones servicioNotificaciones,
            ICacheService cacheService)
        {
            _servicioOriginal = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }
        
        /// <inheritdoc/>
        public async Task<Notificacion> EnviarNotificacionAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            Guid destinatarioId,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            // Esta operación modifica datos, no la cacheamos
            var resultado = await _servicioOriginal.EnviarNotificacionAsync(
                titulo, 
                mensaje, 
                tipo, 
                destinatarioId, 
                entidadRelacionadaId, 
                cancellationToken);
            
            // Invalidar caché relacionada con el destinatario
            InvalidarCachePorDestinatario(destinatarioId);
            
            // Invalidar caché relacionada con el tipo de notificación
            InvalidarCachePorTipo(tipo);
            
            return resultado;
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<Notificacion>> EnviarNotificacionMasivaAsync(
            string titulo,
            string mensaje,
            TipoNotificacion tipo,
            IEnumerable<Guid> destinatarioIds,
            Guid? entidadRelacionadaId = null,
            CancellationToken cancellationToken = default)
        {
            // Esta operación modifica datos, no la cacheamos
            var resultado = await _servicioOriginal.EnviarNotificacionMasivaAsync(
                titulo, 
                mensaje, 
                tipo, 
                destinatarioIds, 
                entidadRelacionadaId, 
                cancellationToken);
            
            // Invalidar caché para cada destinatario
            foreach (var destinatarioId in destinatarioIds)
            {
                InvalidarCachePorDestinatario(destinatarioId);
            }
            
            // Invalidar caché relacionada con el tipo de notificación
            InvalidarCachePorTipo(tipo);
            
            return resultado;
        }
        
        /// <inheritdoc/>
        public async Task<bool> MarcarComoLeidaAsync(
            Guid notificacionId, 
            CancellationToken cancellationToken = default)
        {
            // Esta operación modifica datos, no la cacheamos
            var resultado = await _servicioOriginal.MarcarComoLeidaAsync(notificacionId, cancellationToken);
            
            if (resultado)
            {
                // Dado que no sabemos exactamente qué destinatario o tipo fue afectado,
                // invalidamos toda la caché para asegurar consistencia
                InvalidarCache();
            }
            
            return resultado;
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(
            Guid destinatarioId, 
            bool soloNoLeidas = false, 
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}ObtenerNotificaciones_{destinatarioId}_{soloNoLeidas}";
            
            return await _cacheService.GetOrAddAsync(
                cacheKey,
                async (ct) => await _servicioOriginal.ObtenerNotificacionesAsync(destinatarioId, soloNoLeidas, ct),
                CacheDurationMinutes,
                cancellationToken);
        }
        
        /// <inheritdoc/>
        public void InvalidarCache()
        {
            _cacheService.InvalidatePattern(CacheKeyPrefix);
        }
        
        /// <inheritdoc/>
        public void InvalidarCachePorTipo(TipoNotificacion tipoNotificacion)
        {
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}ObtenerNotificaciones_");
        }
        
        /// <inheritdoc/>
        public void InvalidarCachePorDestinatario(Guid destinatarioId)
        {
            _cacheService.InvalidatePattern($"{CacheKeyPrefix}ObtenerNotificaciones_{destinatarioId}_");
        }
    }
} 