using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Interfaz para la telemetría de caché
    /// </summary>
    public interface ICacheTelemetry
    {
        /// <summary>
        /// Registra un acceso a la caché
        /// </summary>
        /// <param name="key">Clave accedida</param>
        /// <param name="hit">True si fue un acierto, false si fue un fallo</param>
        /// <param name="operationType">Tipo de operación (Get, Set, Remove, etc.)</param>
        /// <param name="elapsedMilliseconds">Tiempo transcurrido en milisegundos</param>
        void TrackCacheAccess(string key, bool hit, string operationType, long elapsedMilliseconds);
        
        /// <summary>
        /// Registra una invalidación de caché
        /// </summary>
        /// <param name="pattern">Patrón invalidado</param>
        /// <param name="keysAffected">Número de claves afectadas</param>
        /// <param name="elapsedMilliseconds">Tiempo transcurrido en milisegundos</param>
        void TrackCacheInvalidation(string pattern, int keysAffected, long elapsedMilliseconds);
        
        /// <summary>
        /// Registra un error en la caché
        /// </summary>
        /// <param name="key">Clave relacionada</param>
        /// <param name="operationType">Tipo de operación (Get, Set, Remove, etc.)</param>
        /// <param name="exception">Excepción ocurrida</param>
        void TrackCacheError(string key, string operationType, Exception exception);
        
        /// <summary>
        /// Obtiene estadísticas actuales de la caché
        /// </summary>
        /// <returns>Diccionario con estadísticas clave-valor</returns>
        Dictionary<string, object> GetStatistics();
    }
} 