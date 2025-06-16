using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RestaurantePro.Infrastructure.Monitoring.HealthChecks;

/// <summary>
/// Health check para verificar la disponibilidad de servicios externos
/// </summary>
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly ExternalServiceHealthCheckOptions _options;

    public ExternalServiceHealthCheck(
        HttpClient httpClient,
        ILogger<ExternalServiceHealthCheck> logger,
        IOptions<ExternalServiceHealthCheckOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var serviceResults = new Dictionary<string, object>();
        var overallStatus = HealthStatus.Healthy;
        var description = "Todos los servicios externos están disponibles";
        
        // Verificamos cada servicio externo configurado
        foreach (var service in _options.ServicesToCheck)
        {
            try
            {
                _logger.LogInformation("Verificando servicio externo: {ServiceName} en {ServiceUrl}", 
                    service.Name, service.HealthCheckUrl);
                
                _httpClient.DefaultRequestHeaders.Clear();
                
                // Agregar headers específicos para este servicio si están configurados
                if (service.Headers != null)
                {
                    foreach (var header in service.Headers)
                    {
                        _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                
                // Configurar el timeout para este servicio específico
                using var cts = new CancellationTokenSource(service.TimeoutMs);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
                
                // Realizar la petición HTTP
                var response = await _httpClient.GetAsync(service.HealthCheckUrl, linkedCts.Token);
                
                // Verificar que la respuesta sea correcta
                if (response.IsSuccessStatusCode)
                {
                    serviceResults[service.Name] = new { 
                        Status = "Healthy",
                        StatusCode = (int)response.StatusCode,
                        ResponseTimeMs = service.TimeoutMs 
                    };
                    
                    _logger.LogInformation("Servicio externo {ServiceName} está disponible", service.Name);
                }
                else
                {
                    serviceResults[service.Name] = new { 
                        Status = "Unhealthy",
                        StatusCode = (int)response.StatusCode,
                        ResponseTimeMs = service.TimeoutMs 
                    };
                    
                    _logger.LogWarning("Servicio externo {ServiceName} no está disponible. Código de estado: {StatusCode}", 
                        service.Name, response.StatusCode);
                    
                    // Ajustar el estado de salud según la importancia del servicio
                    if (service.IsCritical)
                    {
                        overallStatus = HealthStatus.Unhealthy;
                        description = $"Servicio crítico {service.Name} no disponible";
                    }
                    else if (overallStatus != HealthStatus.Unhealthy)
                    {
                        overallStatus = HealthStatus.Degraded;
                        description = $"Servicio no crítico {service.Name} no disponible";
                    }
                }
            }
            catch (OperationCanceledException)
            {
                serviceResults[service.Name] = new { 
                    Status = "Timeout",
                    TimeoutMs = service.TimeoutMs 
                };
                
                _logger.LogWarning("Timeout al verificar servicio externo: {ServiceName}", service.Name);
                
                // Ajustar el estado de salud según la importancia del servicio
                if (service.IsCritical)
                {
                    overallStatus = HealthStatus.Unhealthy;
                    description = $"Timeout en servicio crítico {service.Name}";
                }
                else if (overallStatus != HealthStatus.Unhealthy)
                {
                    overallStatus = HealthStatus.Degraded;
                    description = $"Timeout en servicio no crítico {service.Name}";
                }
            }
            catch (Exception ex)
            {
                serviceResults[service.Name] = new { 
                    Status = "Error",
                    Error = ex.Message 
                };
                
                _logger.LogError(ex, "Error al verificar servicio externo {ServiceName}: {ErrorMessage}", 
                    service.Name, ex.Message);
                
                // Ajustar el estado de salud según la importancia del servicio
                if (service.IsCritical)
                {
                    overallStatus = HealthStatus.Unhealthy;
                    description = $"Error en servicio crítico {service.Name}: {ex.Message}";
                }
                else if (overallStatus != HealthStatus.Unhealthy)
                {
                    overallStatus = HealthStatus.Degraded;
                    description = $"Error en servicio no crítico {service.Name}: {ex.Message}";
                }
            }
        }
        
        return new HealthCheckResult(overallStatus, description, data: serviceResults);
    }
}

/// <summary>
/// Opciones de configuración para la verificación de servicios externos
/// </summary>
public class ExternalServiceHealthCheckOptions
{
    /// <summary>
    /// Lista de servicios externos que se deben verificar
    /// </summary>
    public List<ExternalServiceConfig> ServicesToCheck { get; set; } = new List<ExternalServiceConfig>();
}

/// <summary>
/// Configuración de un servicio externo para su verificación
/// </summary>
public class ExternalServiceConfig
{
    /// <summary>
    /// Nombre del servicio externo
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// URL del endpoint de health check del servicio
    /// </summary>
    public string HealthCheckUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Tiempo máximo de espera para la petición (en milisegundos)
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;
    
    /// <summary>
    /// Indica si este servicio es crítico para la aplicación
    /// </summary>
    public bool IsCritical { get; set; }
    
    /// <summary>
    /// Headers adicionales para la petición HTTP
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
} 