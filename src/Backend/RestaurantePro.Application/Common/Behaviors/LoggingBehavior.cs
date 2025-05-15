using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
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
                _logger.LogError(ex, "Error en solicitud {RequestName}: {ErrorMessage}", requestName, ex.Message);
                throw;
            }
        }
    }
} 