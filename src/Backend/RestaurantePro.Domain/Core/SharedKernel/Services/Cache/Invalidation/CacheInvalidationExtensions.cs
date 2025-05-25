namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Invalidation
{
    /// <summary>
    /// Extensiones para facilitar la invalidación de caché basada en eventos
    /// </summary>
    public static class CacheInvalidationExtensions
    {
        /// <summary>
        /// Obtiene el ID de la entidad relacionada con un evento de dominio
        /// </summary>
        /// <param name="domainEvent">Evento de dominio</param>
        /// <returns>ID de la entidad o null si no se puede determinar</returns>
        public static Guid? GetEntityId(this DomainEvent domainEvent)
        {
            if (domainEvent == null)
                return null;

            // Intentar encontrar una propiedad con el nombre de la entidad + "Id"
            var eventType = domainEvent.GetType();
            var entityName = GetEntityNameFromEvent(eventType);
            
            if (!string.IsNullOrEmpty(entityName))
            {
                var idPropertyName = $"{entityName}Id";
                var idProperty = eventType.GetProperty(idPropertyName, BindingFlags.Public | BindingFlags.Instance);
                
                if (idProperty != null && idProperty.PropertyType == typeof(Guid))
                {
                    return (Guid)idProperty.GetValue(domainEvent);
                }
            }
            
            // Búsqueda alternativa: cualquier propiedad terminada en "Id" que sea Guid
            foreach (var property in eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.Name.EndsWith("Id") && property.PropertyType == typeof(Guid))
                {
                    return (Guid)property.GetValue(domainEvent);
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Intenta extraer el nombre de la entidad a partir del nombre del evento
        /// </summary>
        /// <param name="eventType">Tipo del evento</param>
        /// <returns>Nombre de la entidad o cadena vacía si no se puede determinar</returns>
        private static string GetEntityNameFromEvent(Type eventType)
        {
            var eventName = eventType.Name;
            
            // Patrones comunes: EntidadCreada, EntidadModificada, EntidadEliminada, etc.
            string[] suffixes = { "Creado", "Creada", "Modificado", "Modificada", "Eliminado", "Eliminada", "Actualizado", "Actualizada" };
            
            foreach (var suffix in suffixes)
            {
                if (eventName.EndsWith(suffix) && eventName.Length > suffix.Length)
                {
                    return eventName.Substring(0, eventName.Length - suffix.Length);
                }
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Invalida la caché para un servicio específico relacionado con una entidad
        /// </summary>
        /// <param name="cacheService">Servicio de caché</param>
        /// <param name="servicePrefix">Prefijo del servicio (ej: "ProductoCategoriaService_")</param>
        /// <param name="entityId">ID de la entidad relacionada</param>
        public static void InvalidateForEntity(this ICacheService cacheService, string servicePrefix, Guid entityId)
        {
            if (cacheService == null || string.IsNullOrEmpty(servicePrefix))
                return;
                
            // Invalidar patrón específico para la entidad
            cacheService.InvalidatePattern($"{servicePrefix}*{entityId}*");
        }
        
        /// <summary>
        /// Invalida la caché para un servicio específico relacionado con un evento de dominio
        /// </summary>
        /// <param name="cacheService">Servicio de caché</param>
        /// <param name="servicePrefix">Prefijo del servicio (ej: "ProductoCategoriaService_")</param>
        /// <param name="domainEvent">Evento de dominio</param>
        public static void InvalidateForEvent(this ICacheService cacheService, string servicePrefix, DomainEvent domainEvent)
        {
            if (cacheService == null || domainEvent == null || string.IsNullOrEmpty(servicePrefix))
                return;
                
            // Intentar obtener el ID de la entidad
            var entityId = domainEvent.GetEntityId();
            
            if (entityId.HasValue)
            {
                // Invalidar caché específica para la entidad
                cacheService.InvalidateForEntity(servicePrefix, entityId.Value);
            }
            else
            {
                // Si no podemos determinar el ID, invalidar todo el prefijo
                cacheService.InvalidatePattern(servicePrefix);
            }
        }
    }
} 