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
                "Error procesando {RequestName} con ID {RequestId}: {ErrorMessage}", 
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
            // Excepciones de dominio específicas
            EntityNotFoundException domainNotFound => new NotFoundException(
                domainNotFound.EntityName, 
                domainNotFound.EntityId, 
                domainNotFound),

            BusinessRuleViolationException businessRule => new ValidationException(
                "BusinessRule", 
                new[] { businessRule.Message }),

            DomainException domain => new AppException(
                $"Error de dominio en {requestName}: {domain.Message}",
                domain),

            // Excepciones de validación de FluentValidation
            FluentValidation.ValidationException fluentValidation => new ValidationException(
                fluentValidation.Errors),

            // Excepciones estándar
            ArgumentNullException argNull => new ValidationException(
                argNull.ParamName ?? "Unknown", 
                new[] { "El parámetro no puede ser nulo." }),

            ArgumentException arg => new ValidationException(
                arg.ParamName ?? "Unknown", 
                new[] { arg.Message }),

            InvalidOperationException invalidOp => new ConflictException(
                $"Operación no válida en {requestName}: {invalidOp.Message}",
                invalidOp),

            UnauthorizedAccessException unauthorized => new ForbiddenAccessException(
                $"Acceso denegado para {requestName}: {unauthorized.Message}",
                unauthorized),

            TimeoutException timeout => new AppException(
                $"Timeout en {requestName} (ID: {requestId}): La operación tardó demasiado tiempo.",
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