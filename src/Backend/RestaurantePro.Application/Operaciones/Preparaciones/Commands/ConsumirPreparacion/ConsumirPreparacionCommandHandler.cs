using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Models;
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
            _logger.LogInformation("Consumiendo preparación: {ProductoId}, Cantidad: {Cantidad}", 
                request.ProductoId, request.Cantidad);

            try
            {
                // Verificar primero que haya suficiente cantidad disponible
                var disponibilidad = await _servicioPreparaciones.VerificarDisponibilidadAsync(
                    request.ProductoId, request.Cantidad);
                
                if (!disponibilidad.Succeeded)
                {
                    _logger.LogWarning("No hay suficiente cantidad disponible: {Error}", disponibilidad.Error);
                    return Result.Failure(disponibilidad.Error ?? "No hay suficiente cantidad disponible");
                }
                
                if (!disponibilidad.Value)
                {
                    _logger.LogWarning("No hay suficiente cantidad disponible del producto {ProductoId}", request.ProductoId);
                    return Result.Failure($"No hay suficiente cantidad disponible del producto {request.ProductoId}");
                }

                // Consumir la preparación
                var resultado = await _servicioPreparaciones.ConsumirPreparacionAsync(
                    request.ProductoId, request.Cantidad);
                
                if (!resultado.Succeeded)
                {
                    _logger.LogWarning("Error al consumir la preparación: {Error}", resultado.Error);
                    return Result.Failure(resultado.Error ?? "Error al consumir la preparación");
                }

                _logger.LogInformation("Preparación consumida exitosamente: {ProductoId}, Cantidad: {Cantidad}", 
                    request.ProductoId, request.Cantidad);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consumir la preparación: {ProductoId}", request.ProductoId);
                return Result.Failure($"Error al consumir la preparación: {ex.Message}");
            }
        }
    }
} 