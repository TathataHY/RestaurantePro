using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.MarcarPreparacionDiariaDisponible
{
    /// <summary>
    /// Manejador para el comando de marcar preparación diaria como disponible
    /// </summary>
    public class MarcarPreparacionDiariaDisponibleCommandHandler : IRequestHandler<MarcarPreparacionDiariaDisponibleCommand, Result<PreparacionDiariaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MarcarPreparacionDiariaDisponibleCommandHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public MarcarPreparacionDiariaDisponibleCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<MarcarPreparacionDiariaDisponibleCommandHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja el comando para marcar una preparación diaria como disponible
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> Handle(
            MarcarPreparacionDiariaDisponibleCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("🟢 Marcando preparación diaria como disponible: {PreparacionId}", request.Id);

            try
            {
                // Buscar la preparación diaria existente
                var preparacion = await _context.PreparacionesDiarias.FindAsync(new object[] { request.Id }, cancellationToken);
                if (preparacion == null)
                {
                    _logger.LogWarning("⚠️ Preparación diaria no encontrada: {PreparacionId}", request.Id);
                    return Result.Failure<PreparacionDiariaDto>("Preparación diaria no encontrada");
                }

                // Marcar como disponible
                preparacion.MarcarComoDisponible();

                // Guardar cambios en la base de datos
                await _context.SaveChangesAsync(cancellationToken);

                // Mapear a DTO
                var preparacionDto = _mapper.Map<PreparacionDiariaDto>(preparacion);

                _logger.LogInformation("✅ Preparación diaria marcada como disponible exitosamente: {PreparacionId}", preparacion.Id);

                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al marcar preparación diaria como disponible: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDiariaDto>($"Error al marcar preparación diaria como disponible: {ex.Message}");
            }
        }
    }
} 