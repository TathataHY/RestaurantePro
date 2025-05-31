using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior que monitorea el rendimiento de las operaciones
/// Registra advertencias cuando las operaciones tardan demasiado
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly IMetricsService? _metricsService;
    private readonly PerformanceSettings _settings;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IMetricsService? metricsService = null,
        IOptions<PerformanceSettings>? settings = null)
    {
        _logger = logger;
        _metricsService = metricsService;
        _settings = settings?.Value ?? new PerformanceSettings();
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        var success = false;

        try
        {
            var response = await next();
            stopwatch.Stop();
            success = true;

            var elapsed = stopwatch.Elapsed;
            
            // Registrar métricas
            _metricsService?.RecordExecutionTime(requestName, elapsed, success);
            
            // Determinar umbral específico para la operación
            var threshold = GetThresholdForOperation(requestName);
            
            if (elapsed > threshold)
            {
                var severity = GetSeverityLevel(elapsed, threshold);
                LogPerformanceIssue(requestName, elapsed, threshold, severity);
            }
            else if (_settings.LogAllOperations)
            {
                _logger.LogDebug("⚡ {RequestName} completado en {ElapsedMs}ms", 
                    requestName, 
                    elapsed.TotalMilliseconds);
            }

            // Registrar métricas adicionales
            RecordAdditionalMetrics(requestName, elapsed, success);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            success = false;
            
            // Registrar métricas de error
            _metricsService?.RecordExecutionTime(requestName, stopwatch.Elapsed, success);
            
            _logger.LogError(ex, "❌ Error en {RequestName} después de {ElapsedMs}ms", 
                requestName, 
                stopwatch.Elapsed.TotalMilliseconds);
            
            RecordAdditionalMetrics(requestName, stopwatch.Elapsed, success);
            
            throw;
        }
    }

    /// <summary>
    /// Obtiene el umbral de rendimiento específico para una operación
    /// </summary>
    private TimeSpan GetThresholdForOperation(string requestName)
    {
        // Umbrales específicos por tipo de operación
        return requestName switch
        {
            // Queries rápidas (búsquedas simples)
            var name when name.Contains("Query") && (name.Contains("Por") || name.Contains("Obtener")) 
                => TimeSpan.FromMilliseconds(_settings.QueryThresholdMs),
            
            // Commands simples
            var name when name.Contains("Command") && (name.Contains("Crear") || name.Contains("Actualizar"))
                => TimeSpan.FromSeconds(_settings.CommandThresholdSeconds),
            
            // Operaciones complejas
            var name when name.Contains("Procesar") || name.Contains("Finalizar") || name.Contains("Analisis")
                => TimeSpan.FromSeconds(_settings.ComplexOperationThresholdSeconds),
            
            // Reportes y análisis con ML
            var name when name.Contains("Reporte") || name.Contains("Analisis") || name.Contains("ML")
                => TimeSpan.FromSeconds(_settings.ReportThresholdSeconds),
            
            // Por defecto
            _ => TimeSpan.FromSeconds(_settings.DefaultThresholdSeconds)
        };
    }

    /// <summary>
    /// Determina el nivel de severidad basado en cuánto excede el umbral
    /// </summary>
    private static PerformanceSeverity GetSeverityLevel(TimeSpan elapsed, TimeSpan threshold)
    {
        var ratio = elapsed.TotalMilliseconds / threshold.TotalMilliseconds;
        
        return ratio switch
        {
            >= 5.0 => PerformanceSeverity.Critical,
            >= 3.0 => PerformanceSeverity.High,
            >= 2.0 => PerformanceSeverity.Medium,
            _ => PerformanceSeverity.Low
        };
    }

    /// <summary>
    /// Registra problemas de rendimiento con el nivel apropiado
    /// </summary>
    private void LogPerformanceIssue(string requestName, TimeSpan elapsed, TimeSpan threshold, PerformanceSeverity severity)
    {
        var emoji = severity switch
        {
            PerformanceSeverity.Critical => "🚨",
            PerformanceSeverity.High => "⚠️",
            PerformanceSeverity.Medium => "🐌",
            _ => "⏰"
        };
        
        var logLevel = severity switch
        {
            PerformanceSeverity.Critical => LogLevel.Error,
            PerformanceSeverity.High => LogLevel.Warning,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, 
            "{Emoji} Operación lenta [{Severity}]: {RequestName} tardó {ElapsedMs}ms (umbral: {ThresholdMs}ms, exceso: {ExcessPercentage:F1}%)", 
            emoji,
            severity,
            requestName, 
            elapsed.TotalMilliseconds, 
            threshold.TotalMilliseconds,
            (elapsed.TotalMilliseconds / threshold.TotalMilliseconds - 1) * 100);
    }

    /// <summary>
    /// Registra métricas adicionales para análisis
    /// </summary>
    private void RecordAdditionalMetrics(string requestName, TimeSpan elapsed, bool success)
    {
        if (_metricsService == null) return;

        try
        {
            // Incrementar contador de operaciones
            _metricsService.IncrementCounter($"operation.{requestName.ToLower()}", 
                new[] { $"success:{success}" });

            // Registrar en histograma para análisis de distribución
            _metricsService.RecordHistogram($"operation_duration.{requestName.ToLower()}", 
                elapsed.TotalMilliseconds);

            // Métricas por categoría de operación
            var category = GetOperationCategory(requestName);
            _metricsService.RecordGauge($"performance.{category}.last_duration", 
                elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando métricas adicionales para {RequestName}", requestName);
        }
    }

    /// <summary>
    /// Obtiene la categoría de operación para métricas
    /// </summary>
    private static string GetOperationCategory(string requestName)
    {
        return requestName switch
        {
            var name when name.Contains("Query") => "query",
            var name when name.Contains("Command") => "command",
            var name when name.Contains("Reporte") => "report",
            var name when name.Contains("Analisis") => "analysis",
            _ => "other"
        };
    }
}

/// <summary>
/// Configuración para el comportamiento de rendimiento
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Umbral para queries simples (ms)
    /// </summary>
    public int QueryThresholdMs { get; set; } = 500;

    /// <summary>
    /// Umbral para commands simples (segundos)
    /// </summary>
    public double CommandThresholdSeconds { get; set; } = 2.0;

    /// <summary>
    /// Umbral para operaciones complejas (segundos)
    /// </summary>
    public double ComplexOperationThresholdSeconds { get; set; } = 5.0;

    /// <summary>
    /// Umbral para reportes (segundos)
    /// </summary>
    public double ReportThresholdSeconds { get; set; } = 10.0;

    /// <summary>
    /// Umbral por defecto (segundos)
    /// </summary>
    public double DefaultThresholdSeconds { get; set; } = 3.0;

    /// <summary>
    /// Registrar todas las operaciones (debug)
    /// </summary>
    public bool LogAllOperations { get; set; } = false;

    /// <summary>
    /// Habilitar métricas detalladas
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = true;
}

/// <summary>
/// Niveles de severidad para problemas de rendimiento
/// </summary>
public enum PerformanceSeverity
{
    Low,
    Medium,
    High,
    Critical
} 