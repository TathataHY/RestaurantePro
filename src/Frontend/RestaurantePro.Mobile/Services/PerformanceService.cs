using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio de monitoreo de performance - V4
/// </summary>
public class PerformanceService
{
    private readonly ILogger<PerformanceService> _logger;
    private readonly Dictionary<string, Stopwatch> _timers;
    private readonly Dictionary<string, List<long>> _metrics;

    public PerformanceService(ILogger<PerformanceService> logger)
    {
        _logger = logger;
        _timers = new Dictionary<string, Stopwatch>();
        _metrics = new Dictionary<string, List<long>>();
    }

    #region Timer Methods

    /// <summary>
    /// Inicia un timer para medir performance
    /// </summary>
    public void StartTimer(string operationName)
    {
        if (_timers.ContainsKey(operationName))
        {
            _timers[operationName].Restart();
        }
        else
        {
            _timers[operationName] = Stopwatch.StartNew();
        }

        _logger.LogDebug("Performance timer started: {OperationName}", operationName);
    }

    /// <summary>
    /// Detiene un timer y registra el tiempo
    /// </summary>
    public long StopTimer(string operationName)
    {
        if (!_timers.ContainsKey(operationName))
        {
            _logger.LogWarning("Timer not found: {OperationName}", operationName);
            return 0;
        }

        var timer = _timers[operationName];
        timer.Stop();
        var elapsedMs = timer.ElapsedMilliseconds;

        // Guardar métrica
        if (!_metrics.ContainsKey(operationName))
        {
            _metrics[operationName] = new List<long>();
        }
        _metrics[operationName].Add(elapsedMs);

        _logger.LogInformation("Performance timer stopped: {OperationName} - {ElapsedMs}ms", 
            operationName, elapsedMs);

        return elapsedMs;
    }

    /// <summary>
    /// Obtiene el tiempo promedio de una operación
    /// </summary>
    public double GetAverageTime(string operationName)
    {
        if (!_metrics.ContainsKey(operationName) || !_metrics[operationName].Any())
        {
            return 0;
        }

        return _metrics[operationName].Average();
    }

    /// <summary>
    /// Obtiene el tiempo mínimo de una operación
    /// </summary>
    public long GetMinTime(string operationName)
    {
        if (!_metrics.ContainsKey(operationName) || !_metrics[operationName].Any())
        {
            return 0;
        }

        return _metrics[operationName].Min();
    }

    /// <summary>
    /// Obtiene el tiempo máximo de una operación
    /// </summary>
    public long GetMaxTime(string operationName)
    {
        if (!_metrics.ContainsKey(operationName) || !_metrics[operationName].Any())
        {
            return 0;
        }

        return _metrics[operationName].Max();
    }

    #endregion

    #region Memory Monitoring

    /// <summary>
    /// Obtiene información de memoria actual
    /// </summary>
    public MemoryInfo GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();
        
        return new MemoryInfo
        {
            WorkingSet = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
            VirtualMemory = process.VirtualMemorySize64,
            PeakWorkingSet = process.PeakWorkingSet64,
            PeakVirtualMemory = process.PeakVirtualMemorySize64
        };
    }

    /// <summary>
    /// Registra información de memoria
    /// </summary>
    public void LogMemoryInfo(string context = "Current")
    {
        var memoryInfo = GetMemoryInfo();
        
        _logger.LogInformation("Memory Info - {Context}: WorkingSet={WorkingSet}MB, " +
            "PrivateMemory={PrivateMemory}MB, VirtualMemory={VirtualMemory}MB",
            context,
            memoryInfo.WorkingSet / 1024 / 1024,
            memoryInfo.PrivateMemory / 1024 / 1024,
            memoryInfo.VirtualMemory / 1024 / 1024);
    }

    #endregion

    #region Performance Warnings

    /// <summary>
    /// Verifica si una operación excede el umbral de tiempo
    /// </summary>
    public bool IsOperationSlow(string operationName, long thresholdMs = 1000)
    {
        var averageTime = GetAverageTime(operationName);
        return averageTime > thresholdMs;
    }

    /// <summary>
    /// Obtiene operaciones lentas
    /// </summary>
    public List<string> GetSlowOperations(long thresholdMs = 1000)
    {
        return _metrics.Keys
            .Where(op => IsOperationSlow(op, thresholdMs))
            .ToList();
    }

    /// <summary>
    /// Genera reporte de performance
    /// </summary>
    public PerformanceReport GenerateReport()
    {
        var report = new PerformanceReport
        {
            GeneratedAt = DateTime.UtcNow,
            MemoryInfo = GetMemoryInfo(),
            Operations = new List<OperationMetrics>()
        };

        foreach (var operation in _metrics.Keys)
        {
            report.Operations.Add(new OperationMetrics
            {
                Name = operation,
                AverageTime = GetAverageTime(operation),
                MinTime = GetMinTime(operation),
                MaxTime = GetMaxTime(operation),
                Count = _metrics[operation].Count
            });
        }

        return report;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Limpia métricas antiguas
    /// </summary>
    public void CleanupOldMetrics(int maxEntriesPerOperation = 100)
    {
        foreach (var operation in _metrics.Keys.ToList())
        {
            if (_metrics[operation].Count > maxEntriesPerOperation)
            {
                _metrics[operation] = _metrics[operation]
                    .Skip(_metrics[operation].Count - maxEntriesPerOperation)
                    .ToList();
            }
        }

        _logger.LogDebug("Cleaned up old performance metrics");
    }

    #endregion
}

#region Data Models

/// <summary>
/// Información de memoria
/// </summary>
public class MemoryInfo
{
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public long VirtualMemory { get; set; }
    public long PeakWorkingSet { get; set; }
    public long PeakVirtualMemory { get; set; }
}

/// <summary>
/// Métricas de una operación
/// </summary>
public class OperationMetrics
{
    public string Name { get; set; } = string.Empty;
    public double AverageTime { get; set; }
    public long MinTime { get; set; }
    public long MaxTime { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Reporte de performance
/// </summary>
public class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public MemoryInfo MemoryInfo { get; set; } = new();
    public List<OperationMetrics> Operations { get; set; } = new();
}

#endregion 