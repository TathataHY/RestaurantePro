namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para reintentos automáticos en caso de errores transitorios
/// Optimizado para alta concurrencia y configuración dinámica
/// </summary>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly RetrySettings _retrySettings;
    
    // ThreadLocal para evitar problemas de concurrencia con Random
    private static readonly ThreadLocal<Random> ThreadLocalRandom = new(() => new Random());

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
        
        if (!shouldRetry || !_retrySettings.Enabled)
        {
            return await next();
        }

        var attempt = 0;
        var maxAttempts = _retrySettings.MaxAttempts;
        Exception? lastException = null;
        
        while (true)
        {
            attempt++;
            
            // Verificar cancelación antes de cada intento
            cancellationToken.ThrowIfCancellationRequested();
            
            try
            {
                if (attempt > 1)
                {
                    _logger.LogInformation(
                        "🔄 Reintentando {RequestName} - Intento {Attempt}/{MaxAttempts}",
                        requestName, attempt, maxAttempts);
                }
                
                var result = await next();
                
                // Log de éxito si fue un reintento
                if (attempt > 1)
                {
                    _logger.LogInformation(
                        "✅ {RequestName} exitoso después de {Attempt} intentos",
                        requestName, attempt);
                }
                
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Si la operación fue cancelada, no reintentar
                _logger.LogInformation(
                    "❌ Operación {RequestName} cancelada en intento {Attempt}",
                    requestName, attempt);
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsRetriableException(ex) && !cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                var delay = CalculateDelay(attempt);
                
                _logger.LogWarning(ex,
                    "⚠️ Error transitorio en {RequestName} - Intento {Attempt}/{MaxAttempts}. " +
                    "Reintentando en {DelayMs}ms. Error: {ErrorMessage}",
                    requestName, attempt, maxAttempts, delay.TotalMilliseconds, ex.Message);
                
                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "❌ Operación {RequestName} cancelada durante delay del intento {Attempt}",
                        requestName, attempt);
                    throw;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                // Si no es un error recuperable o ya agotamos los intentos
                if (attempt >= maxAttempts)
                {
                    _logger.LogError(ex,
                        "💥 Falló {RequestName} después de {MaxAttempts} intentos. Error final: {ErrorMessage}",
                        requestName, maxAttempts, ex.Message);
                }
                else
                {
                    _logger.LogError(ex,
                        "❌ Error no recuperable en {RequestName} - Intento {Attempt}. Error: {ErrorMessage}",
                        requestName, attempt, ex.Message);
                }
                
                throw;
            }
        }
    }

    /// <summary>
    /// Determina si se debe aplicar retry a esta operación usando configuración dinámica
    /// </summary>
    private bool ShouldApplyRetry(string requestName)
    {
        // Usar configuración dinámica en lugar de lista hardcodeada
        if (_retrySettings.RetryableCommands.Any(command => 
            requestName.Contains(command, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        // Fallback a lista hardcodeada si no hay configuración
        var fallbackCommands = new[]
        {
            "CrearFactura", "ProcesarPago", "FinalizarComanda", "CrearReservacion",
            "ActualizarStock", "CrearOrdenCompra", "EnviarNotificacion"
        };
        
        return fallbackCommands.Any(command => 
            requestName.Contains(command, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina si una excepción es recuperable (transient failure)
    /// Optimizado con configuración dinámica
    /// </summary>
    private bool IsRetriableException(Exception exception)
    {
        // Primero verificar errores que NO son recuperables
        if (exception is ArgumentException or ArgumentNullException)
            return false;
            
        if (exception.GetType().Name.Contains("Validation") || 
            exception.GetType().Name.Contains("BusinessRule"))
            return false;

        // Verificar por tipo de excepción usando configuración
        var exceptionTypeName = exception.GetType().Name;
        if (_retrySettings.RetryableExceptions.Contains(exceptionTypeName))
        {
            return true;
        }

        // Luego verificar errores que SÍ son recuperables por tipo específico
        return exception switch
        {
            // Errores de red/conexión
            HttpRequestException => true,
            TaskCanceledException when !exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => false, // Solo timeout, no cancelación manual
            TimeoutException => true,
            
            // Errores específicos de la aplicación que son transitorios
            _ when exception.GetType().Name == "InvalidConcurrencyException" => true,
            _ when exception.GetType().Name == "DbUpdateConcurrencyException" => true,
            
            // Errores de base de datos transitorios (por mensaje)
            _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("temporary", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("throttled", StringComparison.OrdinalIgnoreCase) => true,
            _ when exception.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) => true,
            
            // Errores de servicios externos
            _ when exception.GetType().Name.Contains("Service") && 
                   exception.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase) => true,
            
            // Por defecto, no reintentar
            _ => false
        };
    }

    /// <summary>
    /// Calcula el delay usando exponential backoff con jitter mejorado
    /// Optimizado para alta concurrencia y mejor distribución
    /// </summary>
    private TimeSpan CalculateDelay(int attempt)
    {
        // Exponential backoff: baseDelay * 2^(attempt-1)
        var baseDelay = _retrySettings.BaseDelayMs;
        var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);
        
        // Aplicar límite máximo
        var cappedDelay = Math.Min(exponentialDelay, _retrySettings.MaxDelayMs);
        
        // Jitter mejorado: Full Jitter (0% a 100% del delay calculado)
        // Esto distribuye mejor la carga y evita thundering herd más efectivamente
        var random = ThreadLocalRandom.Value!;
        var jitterMultiplier = random.NextDouble(); // 0.0 a 1.0
        var finalDelay = cappedDelay * jitterMultiplier;
        
        // Asegurar un delay mínimo (10% del base delay)
        var minDelay = baseDelay * 0.1;
        finalDelay = Math.Max(finalDelay, minDelay);
        
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
    /// Optimizado para reducir carga bajo alta concurrencia
    /// </summary>
    public double BaseDelayMs { get; set; } = 800; // Reducido de 1000 a 800ms

    /// <summary>
    /// Delay máximo en milisegundos
    /// Optimizado para evitar timeouts excesivos
    /// </summary>
    public double MaxDelayMs { get; set; } = 25000; // Reducido de 30000 a 25000ms

    /// <summary>
    /// Habilitar o deshabilitar reintentos globalmente
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Comandos específicos que deben usar retry
    /// Expandido para incluir operaciones críticas adicionales
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
        "FinalizarServicioCompleto",
        "ActualizarUsuario", // Agregado para mayor cobertura
        "AnularFactura",     // Agregado para operaciones críticas
        "TransferirMesa",    // Agregado para operaciones de mesa
        "UnificarComandas"   // Agregado para operaciones complejas
    };

    /// <summary>
    /// Tipos de excepción que permiten retry
    /// Expandido para mejor detección de errores transitorios
    /// </summary>
    public List<string> RetryableExceptions { get; set; } = new()
    {
        "HttpRequestException",
        "TaskCanceledException", 
        "TimeoutException",
        "InvalidConcurrencyException",
        "DbUpdateConcurrencyException",
        "SqlException",              // Agregado para errores de SQL Server
        "SocketException",          // Agregado para errores de red
        "EndOfStreamException"      // Agregado para errores de conexión
    };
} 