namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para manejo centralizado de excepciones
/// Convierte excepciones de dominio a excepciones de aplicación
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;
            var requestId = Guid.NewGuid();

            _logger.LogError(exception, 
                "Error no controlado procesando {RequestName} con ID {RequestId}: {ErrorMessage}", 
                requestName, requestId, exception.Message);

            // Convertir excepciones de dominio a excepciones de aplicación
            throw MapDomainExceptionToApplicationException(exception, requestName, requestId);
        }
    }

    /// <summary>
    /// Mapea excepciones de dominio a excepciones de aplicación apropiadas
    /// </summary>
    private static Exception MapDomainExceptionToApplicationException(Exception exception, string requestName, Guid requestId)
    {
        return exception switch
        {
            // Excepciones de aplicación que deben mantenerse sin conversión
            RestaurantePro.Application.Common.Exceptions.ValidationException appValidation => appValidation,
            ConflictException => exception,
            NotFoundException => exception,
            ForbiddenAccessException => exception,
            AppException => exception,
            
            // Excepciones del sistema que deben mantenerse
            OperationCanceledException => exception,
            
            // Excepciones de dominio específicas
            EntityNotFoundException domainNotFound => new NotFoundException(
                domainNotFound.EntityName ?? "Unknown", 
                domainNotFound.SearchCriteria ?? "Unknown", 
                domainNotFound.Message),

            BusinessRuleViolationException businessRule => new RestaurantePro.Application.Common.Exceptions.ValidationException(
                businessRule.Message,
                "BusinessRule", 
                businessRule.Message),

            DomainException domain => new RestaurantePro.Application.Common.Exceptions.ValidationException(
                $"Error de dominio: {domain.Message}",
                "Domain", 
                $"Error de dominio: {domain.Message}"),

            // Excepciones de validación de FluentValidation
            FluentValidation.ValidationException fluentValidation => new RestaurantePro.Application.Common.Exceptions.ValidationException(
                fluentValidation.Errors),

            // Excepciones estándar - preservar mensaje original
            ArgumentNullException argNull => new RestaurantePro.Application.Common.Exceptions.ValidationException(
                argNull.Message, // Preservar mensaje original
                argNull.ParamName ?? "Unknown", 
                argNull.Message),

            ArgumentException arg => new RestaurantePro.Application.Common.Exceptions.ValidationException(
                arg.Message, // Preservar mensaje original
                arg.ParamName ?? "Unknown", 
                arg.Message),

            InvalidOperationException invalidOp => new AppException(
                $"Operación no válida en {requestName}: {invalidOp.Message}",
                invalidOp),

            UnauthorizedAccessException unauthorized => new ForbiddenAccessException(
                $"Acceso denegado para {requestName}: {unauthorized.Message}"),

            TimeoutException timeout => new AppException(
                timeout.Message, // Preservar mensaje original
                timeout),

            // Excepciones de concurrencia
            InvalidConcurrencyException concurrency => new ConflictException(
                $"Conflicto de concurrencia en {requestName}: {concurrency.Message}",
                concurrency),

            // Exception genérica (fallback)
            _ => new AppException(
                $"Error inesperado en {requestName} (ID: {requestId}): {exception.Message}",
                exception)
        };
    }
}

/// <summary>
/// Excepción para errores de concurrencia/conflictos de estado
/// </summary>
public class InvalidConcurrencyException : Exception
{
    public InvalidConcurrencyException(string message) : base(message) { }
    
    public InvalidConcurrencyException(string message, Exception innerException) 
        : base(message, innerException) { }
} 