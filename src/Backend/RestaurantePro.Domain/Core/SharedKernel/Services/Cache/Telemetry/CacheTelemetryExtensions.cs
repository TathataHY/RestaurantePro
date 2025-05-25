namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry
{
    /// <summary>
    /// Extensiones para facilitar el uso de la telemetría de caché
    /// </summary>
    public static class CacheTelemetryExtensions
    {
        /// <summary>
        /// Obtiene un informe en formato legible de las métricas de caché
        /// </summary>
        /// <param name="telemetry">Instancia de telemetría</param>
        /// <returns>Informe en formato de texto</returns>
        public static string GenerarInformeTelemetria(this ICacheTelemetry telemetry)
        {
            if (telemetry == null)
                return "No hay telemetría disponible";
            
            var sb = new StringBuilder();
            var metrics = telemetry.GetMetrics();
            
            sb.AppendLine("=== INFORME DE TELEMETRÍA DE CACHÉ ===");
            sb.AppendLine($"Total de accesos: {metrics.TotalAccesses}");
            sb.AppendLine($"Total de aciertos: {metrics.TotalHits}");
            sb.AppendLine($"Tasa de aciertos: {metrics.HitRate:P2}");
            sb.AppendLine($"Tiempo promedio de acceso: {metrics.AverageAccessTimeMs:F2} ms");
            sb.AppendLine($"Invalidaciones recientes: {metrics.RecentInvalidations}");
            
            if (metrics.RecentErrors.Any())
            {
                sb.AppendLine("\n=== ERRORES RECIENTES ===");
                foreach (var error in metrics.RecentErrors.Take(10))
                {
                    sb.AppendLine(error);
                }
                
                if (metrics.RecentErrors.Count > 10)
                {
                    sb.AppendLine($"... y {metrics.RecentErrors.Count - 10} errores más");
                }
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Obtiene la telemetría de caché del contenedor de servicios
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios</param>
        /// <returns>Servicio de telemetría de caché</returns>
        public static ICacheTelemetry GetCacheTelemetry(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ICacheTelemetry>();
        }
    }
} 