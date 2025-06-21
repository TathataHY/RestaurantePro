namespace RestaurantePro.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            
            _logger.LogInformation("Iniciando solicitud {RequestName}", requestName);
            
            try
            {
                var result = await next();
                
                _logger.LogInformation("Solicitud completada {RequestName}", requestName);
                
                return result;
            }
            catch (Exception ex)
            {
                // Las ValidationException son parte normal del flujo de validación, no errores del sistema
                if (ex is RestaurantePro.Application.Common.Exceptions.ValidationException)
                {
                    _logger.LogWarning(ex, "Validación fallida en solicitud {RequestName}: {ErrorMessage}", requestName, ex.Message);
                }
                else
                {
                    _logger.LogError(ex, "Error en solicitud {RequestName}: {ErrorMessage}", requestName, ex.Message);
                }
                throw;
            }
        }
    }
} 