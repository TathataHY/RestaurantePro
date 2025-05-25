using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Extensiones para facilitar el uso de la telemetría de caché
    /// </summary>
    public static class CacheTelemetryExtensions
    {
        /// <summary>
        /// Obtiene un informe de estadísticas de la caché en formato legible
        /// </summary>
        /// <param name="telemetry">Servicio de telemetría</param>
        /// <returns>Informe en formato de texto</returns>
        public static string GetHumanReadableReport(this ICacheTelemetry telemetry)
        {
            if (telemetry == null)
                throw new ArgumentNullException(nameof(telemetry));
                
            var stats = telemetry.GetStatistics();
            var sb = new StringBuilder();
            
            sb.AppendLine("=== INFORME DE TELEMETRÍA DE CACHÉ ===");
            sb.AppendLine();
            
            // Información general
            sb.AppendLine("INFORMACIÓN GENERAL:");
            sb.AppendLine($"Tiempo activo: {stats["Uptime"]}");
            sb.AppendLine($"Total aciertos: {stats["TotalHits"]}");
            sb.AppendLine($"Total fallos: {stats["TotalMisses"]}");
            sb.AppendLine($"Tasa de aciertos: {(double)stats["HitRate"]:P2}");
            sb.AppendLine($"Total invalidaciones: {stats["TotalInvalidations"]}");
            sb.AppendLine($"Total claves afectadas: {stats["TotalKeysAffected"]}");
            sb.AppendLine($"Total errores: {stats["TotalErrors"]}");
            sb.AppendLine();
            
            // Top claves con más aciertos
            sb.AppendLine("TOP 10 CLAVES CON MÁS ACIERTOS:");
            var topHits = (Dictionary<string, long>)stats["TopHitKeys"];
            foreach (var hit in topHits)
            {
                sb.AppendLine($"  {hit.Key}: {hit.Value} aciertos");
            }
            sb.AppendLine();
            
            // Top claves con más fallos
            sb.AppendLine("TOP 10 CLAVES CON MÁS FALLOS:");
            var topMisses = (Dictionary<string, long>)stats["TopMissKeys"];
            foreach (var miss in topMisses)
            {
                sb.AppendLine($"  {miss.Key}: {miss.Value} fallos");
            }
            sb.AppendLine();
            
            // Top patrones de invalidación
            sb.AppendLine("TOP 10 PATRONES DE INVALIDACIÓN:");
            var topInvalidations = (Dictionary<string, long>)stats["TopInvalidationPatterns"];
            foreach (var invalidation in topInvalidations)
            {
                sb.AppendLine($"  {invalidation.Key}: {invalidation.Value} invalidaciones");
            }
            sb.AppendLine();
            
            // Tiempos promedio por operación
            sb.AppendLine("TIEMPOS PROMEDIO POR OPERACIÓN:");
            var avgDurations = (Dictionary<string, double>)stats["AverageDurationMs"];
            foreach (var duration in avgDurations)
            {
                sb.AppendLine($"  {duration.Key}: {duration.Value:F2} ms");
            }
            sb.AppendLine();
            
            // Errores recientes
            sb.AppendLine("ERRORES RECIENTES:");
            var recentErrors = (List<dynamic>)stats["RecentErrors"];
            foreach (var error in recentErrors)
            {
                sb.AppendLine($"  {error.Timestamp}: {error.OperationType} - {error.Key} - {error.Error}");
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