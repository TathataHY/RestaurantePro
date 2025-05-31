namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para auditoría automática de commands
/// Registra quién, cuándo y qué operaciones se realizan
/// </summary>
public class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService? _currentUserService;

    public AuditingBehavior(
        ILogger<AuditingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService? currentUserService = null)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var auditId = Guid.NewGuid();
        
        // Solo auditar Commands (que modifican datos), no Queries
        var isCommand = IsCommand(requestName);
        
        if (isCommand)
        {
            await LogAuditStart(request, requestName, auditId);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            if (isCommand)
            {
                await LogAuditSuccess(request, response, requestName, auditId, stopwatch.ElapsedMilliseconds);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            if (isCommand)
            {
                await LogAuditFailure(request, requestName, auditId, stopwatch.ElapsedMilliseconds, ex);
            }
            throw;
        }
    }

    /// <summary>
    /// Determina si es un command (modifica datos) basado en el nombre
    /// </summary>
    private static bool IsCommand(string requestName)
    {
        var commandSuffixes = new[] { "Command", "Handler" };
        var queryIndicators = new[] { "Query", "Obtener", "Buscar", "Consultar", "Get", "Find", "Search" };
        
        // Si contiene indicadores de query, no es command
        if (queryIndicators.Any(indicator => requestName.Contains(indicator, StringComparison.OrdinalIgnoreCase)))
            return false;
            
        // Si contiene sufijos de command, es command
        if (commandSuffixes.Any(suffix => requestName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            return true;
            
        // Operaciones típicas de command
        var commandIndicators = new[] { "Crear", "Actualizar", "Eliminar", "Modificar", "Agregar", "Remover", 
                                       "Create", "Update", "Delete", "Add", "Remove", "Aplicar", "Procesar", 
                                       "Finalizar", "Cancelar", "Confirmar", "Anular" };
        
        return commandIndicators.Any(indicator => requestName.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Registra el inicio de una operación auditable
    /// </summary>
    private async Task LogAuditStart(TRequest request, string requestName, Guid auditId)
    {
        try
        {
            var userId = await GetCurrentUserId();
            var requestData = SerializeRequest(request);
            
            _logger.LogInformation(
                "AUDIT_START: {AuditId} | Usuario: {UserId} | Operación: {Operation} | Datos: {RequestData} | Timestamp: {Timestamp}",
                auditId, userId, requestName, requestData, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando inicio de auditoría para {RequestName}", requestName);
        }
    }

    /// <summary>
    /// Registra una operación exitosa
    /// </summary>
    private async Task LogAuditSuccess(TRequest request, TResponse response, string requestName, Guid auditId, long elapsedMs)
    {
        try
        {
            var userId = await GetCurrentUserId();
            var responseData = SerializeResponse(response);
            
            _logger.LogInformation(
                "AUDIT_SUCCESS: {AuditId} | Usuario: {UserId} | Operación: {Operation} | Resultado: {ResponseData} | Duración: {ElapsedMs}ms | Timestamp: {Timestamp}",
                auditId, userId, requestName, responseData, elapsedMs, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando éxito de auditoría para {RequestName}", requestName);
        }
    }

    /// <summary>
    /// Registra una operación fallida
    /// </summary>
    private async Task LogAuditFailure(TRequest request, string requestName, Guid auditId, long elapsedMs, Exception exception)
    {
        try
        {
            var userId = await GetCurrentUserId();
            
            _logger.LogError(
                "AUDIT_FAILURE: {AuditId} | Usuario: {UserId} | Operación: {Operation} | Error: {ErrorMessage} | Duración: {ElapsedMs}ms | Timestamp: {Timestamp}",
                auditId, userId, requestName, exception.Message, elapsedMs, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando fallo de auditoría para {RequestName}", requestName);
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual
    /// </summary>
    private async Task<string> GetCurrentUserId()
    {
        try
        {
            if (_currentUserService != null)
            {
                return _currentUserService.GetUserId() ?? "Sistema";
            }
            return "Sistema";
        }
        catch
        {
            return "Desconocido";
        }
    }

    /// <summary>
    /// Serializa el request de forma segura
    /// </summary>
    private static string SerializeRequest(TRequest request)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            return JsonSerializer.Serialize(request, options);
        }
        catch
        {
            return $"[{typeof(TRequest).Name} - No serializable]";
        }
    }

    /// <summary>
    /// Serializa la response de forma segura
    /// </summary>
    private static string SerializeResponse(TResponse response)
    {
        try
        {
            // Para Results, solo mostrar si fue exitoso
            if (response?.GetType().Name.Contains("Result") == true)
            {
                var successProperty = response.GetType().GetProperty("Succeeded");
                if (successProperty != null)
                {
                    var isSuccess = (bool?)successProperty.GetValue(response) ?? false;
                    return isSuccess ? "Success" : "Failed";
                }
            }
            
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            return JsonSerializer.Serialize(response, options);
        }
        catch
        {
            return $"[{typeof(TResponse).Name} - No serializable]";
        }
    }
} 