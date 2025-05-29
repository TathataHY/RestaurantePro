using System.Diagnostics;
using MediatR;

namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior que monitorea el rendimiento de las operaciones
/// Registra advertencias cuando las operaciones tardan demasiado
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _warningThreshold;

    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _warningThreshold = TimeSpan.FromSeconds(3); // 3 segundos como umbral de advertencia
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;
            if (elapsed > _warningThreshold)
            {
                _logger.LogWarning("🐌 Operación lenta detectada: {RequestName} tardó {ElapsedMilliseconds}ms (umbral: {ThresholdMs}ms)", 
                    requestName, 
                    elapsed.TotalMilliseconds, 
                    _warningThreshold.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug("⚡ {RequestName} completado en {ElapsedMilliseconds}ms", 
                    requestName, 
                    elapsed.TotalMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error en {RequestName} después de {ElapsedMilliseconds}ms", 
                requestName, 
                stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
} 