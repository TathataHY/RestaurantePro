namespace RestaurantePro.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v => 
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Any())
                {
                    _logger.LogWarning("⚠️ Errores de validación en {RequestType}:", typeof(TRequest).Name);
                    foreach (var failure in failures)
                    {
                        _logger.LogWarning("   - {PropertyName}: {ErrorMessage}", failure.PropertyName, failure.ErrorMessage);
                    }
                    
                    var errorMessages = failures.Select(f => f.ErrorMessage).ToList();
                    var errorMessage = string.Join("; ", errorMessages);
                    
                    var failureResult = CreateFailureResult<TResponse>(errorMessage);
                    return failureResult;
                }
            }

            return await next();
        }

        /// <summary>
        /// Crea un Result.Failure del tipo correcto usando reflexión
        /// </summary>
        private static TResponse CreateFailureResult<TResponse>(string errorMessage)
        {
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericType = typeof(TResponse).GetGenericArguments()[0];
                var resultType = typeof(Result);
                var failureMethod = resultType.GetMethods()
                    .FirstOrDefault(m => m.Name == "Failure" && m.IsGenericMethod && m.GetParameters().Length == 0);
                if (failureMethod != null)
                {
                    var genericFailure = failureMethod.MakeGenericMethod(genericType);
                    return (TResponse)genericFailure.Invoke(null, null)!;
                }
            }
            throw new RestaurantePro.Application.Common.Exceptions.ValidationException(errorMessage, "Validation", errorMessage);
        }
    }
} 