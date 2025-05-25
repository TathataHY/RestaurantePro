namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry
{
    /// <summary>
    /// Interfaz para la telemetría del servicio de caché
    /// </summary>
    public interface ICacheTelemetry
    {
        /// <summary>
        /// Registra el acceso a la caché
        /// </summary>
        /// <param name="key">Clave accedida</param>
        /// <param name="isHit">Si fue un acierto o no</param>
        /// <param name="operationType">Tipo de operación</param>
        /// <param name="elapsedMs">Tiempo de ejecución en milisegundos</param>
        void TrackCacheAccess(string key, bool isHit, string operationType, long elapsedMs);
        
        /// <summary>
        /// Registra un error en la caché
        /// </summary>
        /// <param name="key">Clave que provocó el error</param>
        /// <param name="operationType">Tipo de operación</param>
        /// <param name="exception">Excepción ocurrida</param>
        void TrackCacheError(string key, string operationType, Exception exception);
        
        /// <summary>
        /// Registra una invalidación de caché
        /// </summary>
        /// <param name="pattern">Patrón invalidado</param>
        /// <param name="keysAffected">Número de claves afectadas</param>
        /// <param name="elapsedMs">Tiempo de ejecución en milisegundos</param>
        void TrackCacheInvalidation(string pattern, int keysAffected, long elapsedMs);
        
        /// <summary>
        /// Obtiene las métricas actuales de la caché
        /// </summary>
        /// <returns>Métricas de la caché</returns>
        CacheMetrics GetMetrics();
    }
    
    /// <summary>
    /// Métricas de la caché
    /// </summary>
    public class CacheMetrics
    {
        /// <summary>
        /// Número total de accesos a la caché
        /// </summary>
        public long TotalAccesses { get; set; }
        
        /// <summary>
        /// Número total de aciertos en la caché
        /// </summary>
        public long TotalHits { get; set; }
        
        /// <summary>
        /// Tasa de aciertos (hits / accesos)
        /// </summary>
        public double HitRate => TotalAccesses > 0 ? (double)TotalHits / TotalAccesses : 0;
        
        /// <summary>
        /// Tiempo promedio de acceso en milisegundos
        /// </summary>
        public double AverageAccessTimeMs { get; set; }
        
        /// <summary>
        /// Número de invalidaciones recientes
        /// </summary>
        public int RecentInvalidations { get; set; }
        
        /// <summary>
        /// Lista de errores recientes
        /// </summary>
        public List<string> RecentErrors { get; set; } = new List<string>();
    }
} 