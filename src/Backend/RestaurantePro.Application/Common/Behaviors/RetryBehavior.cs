namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para reintentos automáticos en caso de errores transitorios
/// </summary>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly RetrySettings _retrySettings;

    public RetryBehavior(
        ILogger<RetryBehavior<TRequest, TResponse>> logger,
        IOptions<RetrySettings>? retrySettings = null)
    {
        _logger = logger;
        _retrySettings = retrySettings?.Value ?? new RetrySettings();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Solo aplicar retry a Commands críticos, no a Queries
        var shouldRetry = ShouldApplyRetry(requestName);
        
        if (!shouldRetry)
        {
            return await next();
        }

        var attempt = 0;
        var maxAttempts = _retrySettings.MaxAttempts;
        
        while (true)
        {
            attempt++;
            
            try
            {
                if (attempt > 1)
                {
                    _logger.LogInformation(
                        "Reintentando {RequestName} - Intento {Attempt}/{MaxAttempts}",
                        requestName, attempt, maxAttempts);
                }
                
                return await next();
            }
            catch (Exception ex) when (attempt < maxAttempts && IsRetriableException(ex))
            {
                var delay = CalculateDelay(attempt);
                
                _logger.LogWarning(ex,
                    "Error transitorio en {RequestName} - Intento {Attempt}/{MaxAttempts}. " +
                    "Reintentando en {DelayMs}ms. Error: {ErrorMessage}",
                    requestName, attempt, maxAttempts, delay.TotalMilliseconds, ex.Message);
                
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                // Si no es un error recuperable o ya agotamos los intentos
                if (attempt >= maxAttempts)
                {
                    _logger.LogError(ex,
                        "Falló {RequestName} después de {MaxAttempts} intentos. Error final: {ErrorMessage}",
                        requestName, maxAttempts, ex.Message);
                }
                
                throw;
            }
        }
    }

    /// <summary>
    /// Determina si se debe aplicar retry a esta operación
    /// </summary>
    private static bool ShouldApplyRetry(string requestName)
    {
        // Aplicar retry solo a Commands críticos
        var criticalCommands = new[]
        {
            "CrearFactura", "ProcesarPago", "FinalizarComanda", "CrearReservacion",
            "ActualizarStock", "CrearOrdenCompra", "EnviarNotificacion"
        };
        
        return criticalCommands.Any(command => 
            requestName.Contains(command, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina si una excepción es recuperable (transient failure)
    /// </summary>
    private static bool IsRetriableException(Exception exception)
    {
        // Primero verificar errores que NO son recuperables
        if (exception is ArgumentException or ArgumentNullException)
            return false;
            
        if (exception.GetType().Name.Contains("Validation") || 
            exception.GetType().Name.Contains("BusinessRule"))
            return false;

        // Luego verificar errores que SÍ son recuperables
        return exception switch
        {
            // Errores de red/conexión
            HttpRequestException => true,
            TaskCanceledException => true,
            TimeoutException => true,
            
            // Errores específicos de la aplicación que son transitorios
            _ when exception.GetType().Name == "InvalidConcurrencyException" => true,
            
            // Errores de base de datos transitorios (por mensaje)
            _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("temporary", StringComparison.OrdinalIgnoreCase) => true,
            
            // Errores de servicios externos
            _ when exception.GetType().Name.Contains("Service") && 
                   exception.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase) => true,
            
            // Por defecto, no reintentar
            _ => false
        };
    }

    /// <summary>
    /// Calcula el delay usando exponential backoff con jitter
    /// </summary>
    private TimeSpan CalculateDelay(int attempt)
    {
        // Exponential backoff: baseDelay * 2^(attempt-1)
        var baseDelay = _retrySettings.BaseDelayMs;
        var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);
        
        // Aplicar límite máximo
        var cappedDelay = Math.Min(exponentialDelay, _retrySettings.MaxDelayMs);
        
        // Agregar jitter (variación aleatoria) para evitar "thundering herd"
        var jitter = Random.Shared.NextDouble() * 0.1; // ±10%
        var finalDelay = cappedDelay * (1 + jitter);
        
        return TimeSpan.FromMilliseconds(finalDelay);
    }
}

/// <summary>
/// Configuración para el comportamiento de reintentos
/// </summary>
public class RetrySettings
{
    /// <summary>
    /// Máximo número de intentos (incluyendo el original)
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Delay base en milisegundos para el primer reintento
    /// </summary>
    public double BaseDelayMs { get; set; } = 1000; // 1 segundo

    /// <summary>
    /// Delay máximo en milisegundos
    /// </summary>
    public double MaxDelayMs { get; set; } = 30000; // 30 segundos

    /// <summary>
    /// Habilitar o deshabilitar reintentos globalmente
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Comandos específicos que deben usar retry
    /// </summary>
    public List<string> RetryableCommands { get; set; } = new()
    {
        "CrearFactura",
        "ProcesarPago", 
        "FinalizarComanda",
        "CrearReservacion",
        "ActualizarStock",
        "CrearOrdenCompra",
        "EnviarNotificacion",
        "ProcesarPedidoCompleto",
        "FinalizarServicioCompleto"
    };

    /// <summary>
    /// Tipos de excepción que permiten retry
    /// </summary>
    public List<string> RetryableExceptions { get; set; } = new()
    {
        "HttpRequestException",
        "TaskCanceledException", 
        "TimeoutException",
        "InvalidConcurrencyException"
    };
} 