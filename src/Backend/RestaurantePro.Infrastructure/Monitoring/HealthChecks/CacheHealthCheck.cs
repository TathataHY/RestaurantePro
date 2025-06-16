using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Infrastructure.Monitoring.HealthChecks;

/// <summary>
/// Health check para verificar la disponibilidad del servicio de caché
/// </summary>
public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheHealthCheck> _logger;
    private const string TestKey = "HealthCheck_Test_Key";
    private const string TestValue = "Cache is working!";

    public CacheHealthCheck(
        ICacheService cacheService,
        ILogger<CacheHealthCheck> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verificando servicio de caché");
            
            // Intentar escribir un valor en caché
            await _cacheService.SetAsync(TestKey, TestValue, TimeSpan.FromMinutes(1), cancellationToken);
            
            // Intentar leer el valor previamente escrito
            var retrievedValue = await _cacheService.GetAsync<string>(TestKey, cancellationToken);
            
            if (retrievedValue == TestValue)
            {
                _logger.LogInformation("Servicio de caché está funcionando correctamente");
                
                // Eliminar el valor de prueba de la caché
                await _cacheService.RemoveAsync(TestKey, cancellationToken);
                
                // Obtener estadísticas del servicio de caché si están disponibles
                var cacheStats = await GetCacheStatisticsAsync();
                
                return HealthCheckResult.Healthy("Servicio de caché disponible y funcionando correctamente", cacheStats);
            }
            else
            {
                _logger.LogWarning("El valor recuperado de la caché no coincide con el valor almacenado. Esperado: {Expected}, Obtenido: {Actual}", 
                    TestValue, retrievedValue);
                return HealthCheckResult.Degraded("El servicio de caché no está funcionando correctamente: inconsistencia en lectura/escritura");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar el servicio de caché: {ErrorMessage}", ex.Message);
            return HealthCheckResult.Unhealthy($"Error en el servicio de caché: {ex.Message}");
        }
    }
    
    private async Task<object> GetCacheStatisticsAsync()
    {
        try
        {
            // Intentar obtener estadísticas del caché si el servicio lo soporta
            if (_cacheService is IProvidesCacheStatistics statsProvider)
            {
                return await statsProvider.GetStatisticsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron obtener estadísticas del servicio de caché: {ErrorMessage}", ex.Message);
        }
        
        return new { };
    }
}

/// <summary>
/// Interfaz para proveedores de caché que pueden proporcionar estadísticas
/// </summary>
public interface IProvidesCacheStatistics
{
    /// <summary>
    /// Obtiene estadísticas del servicio de caché
    /// </summary>
    /// <returns>Objeto con estadísticas del caché</returns>
    Task<object> GetStatisticsAsync();
} 