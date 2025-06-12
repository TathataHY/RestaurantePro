using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarComoDisponible
{
    /// <summary>
    /// Manejador para el comando de marcar una preparación como disponible
    /// </summary>
    public class MarcarComoDisponibleCommandHandler : IRequestHandler<MarcarComoDisponibleCommand, Result>
    {
        private readonly IServicioPreparaciones _servicioPreparaciones;
        private readonly IMapper _mapper;
        private readonly ILogger<MarcarComoDisponibleCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public MarcarComoDisponibleCommandHandler(
            IServicioPreparaciones servicioPreparaciones,
            IMapper mapper,
            ILogger<MarcarComoDisponibleCommandHandler> logger)
        {
            _servicioPreparaciones = servicioPreparaciones ?? throw new ArgumentNullException(nameof(servicioPreparaciones));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maneja el comando para marcar una preparación como disponible
        /// </summary>
        public async Task<Result> Handle(
            MarcarComoDisponibleCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Marcando preparación como disponible: {PreparacionId}", request.PreparacionId);

            try
            {
                // Llamar al servicio de dominio para marcar la preparación como disponible
                var resultadoMarcado = await _servicioPreparaciones.MarcarComoDisponibleAsync(request.PreparacionId);

                if (!resultadoMarcado.Succeeded)
                {
                    _logger.LogWarning("No se pudo marcar la preparación como disponible: {Error}", resultadoMarcado.Error);
                    return Result.Failure(resultadoMarcado.Error ?? "Error desconocido al marcar como disponible");
                }

                _logger.LogInformation("Preparación marcada como disponible exitosamente: {PreparacionId}", request.PreparacionId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar la preparación como disponible: {PreparacionId}", request.PreparacionId);
                return Result.Failure($"Error al marcar la preparación como disponible: {ex.Message}");
            }
        }
    }
} 
