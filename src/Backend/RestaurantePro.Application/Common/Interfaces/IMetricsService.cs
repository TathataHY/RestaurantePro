namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para recopilar y enviar métricas de performance
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Registra el tiempo de ejecución de una operación
    /// </summary>
    void RecordExecutionTime(string operationName, TimeSpan duration, bool success = true);

    /// <summary>
    /// Incrementa un contador de operaciones
    /// </summary>
    void IncrementCounter(string counterName, string[]? tags = null);

    /// <summary>
    /// Registra una métrica de gauge (valor instantáneo)
    /// </summary>
    void RecordGauge(string gaugeName, double value, string[]? tags = null);

    /// <summary>
    /// Registra una métrica de histograma
    /// </summary>
    void RecordHistogram(string histogramName, double value, string[]? tags = null);

    /// <summary>
    /// Registra métricas de uso de memoria
    /// </summary>
    void RecordMemoryUsage(long bytesUsed, string context);

    /// <summary>
    /// Registra métricas de base de datos
    /// </summary>
    void RecordDatabaseMetrics(string operation, TimeSpan duration, bool success, int recordsAffected = 0);

    /// <summary>
    /// Registra métricas de cache
    /// </summary>
    void RecordCacheMetrics(string operation, bool hit, TimeSpan? duration = null);

    /// <summary>
    /// Registra métricas de servicios externos
    /// </summary>
    void RecordExternalServiceMetrics(string serviceName, string operation, TimeSpan duration, bool success, int? statusCode = null);

    /// <summary>
    /// Registra métricas de eventos de dominio
    /// </summary>
    void RecordDomainEventMetrics(string eventType, TimeSpan processingTime, bool success);

    /// <summary>
    /// Registra métricas de background jobs
    /// </summary>
    void RecordBackgroundJobMetrics(string jobType, TimeSpan duration, bool success);

    /// <summary>
    /// Registra métricas de validación
    /// </summary>
    void RecordValidationMetrics(string validationType, bool success, int errorCount = 0);

    /// <summary>
    /// Registra métricas de usuario
    /// </summary>
    void RecordUserMetrics(string action, string? userId = null, string? role = null);

    /// <summary>
    /// Registra métricas de negocio específicas del restaurante
    /// </summary>
    void RecordBusinessMetrics(string metric, double value, string context);

    /// <summary>
    /// Obtiene un resumen de métricas actuales
    /// </summary>
    MetricsSummary GetMetricsSummary();
}

/// <summary>
/// Resumen de métricas del sistema
/// </summary>
public class MetricsSummary
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, OperationMetrics> Operations { get; set; } = new();
    public SystemMetrics System { get; set; } = new();
    public BusinessMetrics Business { get; set; } = new();
}

/// <summary>
/// Métricas de una operación específica
/// </summary>
public class OperationMetrics
{
    public string OperationName { get; set; } = string.Empty;
    public long TotalExecutions { get; set; }
    public long SuccessfulExecutions { get; set; }
    public long FailedExecutions { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public TimeSpan MinExecutionTime { get; set; }
    public TimeSpan MaxExecutionTime { get; set; }
    public double SuccessRate => TotalExecutions > 0 ? (double)SuccessfulExecutions / TotalExecutions * 100 : 0;
}

/// <summary>
/// Métricas del sistema
/// </summary>
public class SystemMetrics
{
    public long MemoryUsageBytes { get; set; }
    public double CpuUsagePercentage { get; set; }
    public long ActiveConnections { get; set; }
    public TimeSpan Uptime { get; set; }
    public Dictionary<string, double> CacheHitRates { get; set; } = new();
}

/// <summary>
/// Métricas de negocio del restaurante
/// </summary>
public class BusinessMetrics
{
    public int ActiveTables { get; set; }
    public int TotalReservations { get; set; }
    public int ProcessingOrders { get; set; }
    public decimal TotalSales { get; set; }
    public int LowStockItems { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public Dictionary<string, decimal> SalesByCategory { get; set; } = new();
} 