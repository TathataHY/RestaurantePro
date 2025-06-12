using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ConsumirPreparacion
{
    /// <summary>
    /// Manejador para el comando de consumir una preparación
    /// </summary>
    public class ConsumirPreparacionCommandHandler : IRequestHandler<ConsumirPreparacionCommand, Result>
    {
        private readonly IServicioPreparaciones _servicioPreparaciones;
        private readonly ILogger<ConsumirPreparacionCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConsumirPreparacionCommandHandler(
            IServicioPreparaciones servicioPreparaciones,
            ILogger<ConsumirPreparacionCommandHandler> logger)
        {
            _servicioPreparaciones = servicioPreparaciones ?? throw new ArgumentNullException(nameof(servicioPreparaciones));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maneja el comando para consumir una preparación
        /// </summary>
        public async Task<Result> Handle(
            ConsumirPreparacionCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consumiendo preparación: {PreparacionId}, Cantidad: {Cantidad}", 
                request.PreparacionId, request.Cantidad);

            try
            {
                // Consumir la preparación
                var resultado = await _servicioPreparaciones.ConsumirPreparacionAsync(
                    request.PreparacionId, request.Cantidad);
                
                if (!resultado.Succeeded)
                {
                    _logger.LogWarning("Error al consumir la preparación: {Error}", resultado.Error);
                    return Result.Failure(resultado.Error ?? "Error al consumir la preparación");
                }

                _logger.LogInformation("Preparación consumida exitosamente: {PreparacionId}, Cantidad: {Cantidad}", 
                    request.PreparacionId, request.Cantidad);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la preparación: {PreparacionId}", request.PreparacionId);
                return Result.Failure($"Error al consumir la preparación: {ex.Message}");
            }
        }
    }
} 

