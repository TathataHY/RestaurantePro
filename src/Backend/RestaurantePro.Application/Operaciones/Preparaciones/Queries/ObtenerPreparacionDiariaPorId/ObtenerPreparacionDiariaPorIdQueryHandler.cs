using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionDiariaPorId
{
    /// <summary>
    /// Manejador para la query de obtener preparación diaria por ID
    /// </summary>
    public class ObtenerPreparacionDiariaPorIdQueryHandler : IRequestHandler<ObtenerPreparacionDiariaPorIdQuery, Result<PreparacionDiariaDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ObtenerPreparacionDiariaPorIdQueryHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObtenerPreparacionDiariaPorIdQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ObtenerPreparacionDiariaPorIdQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja la query para obtener una preparación diaria específica por ID
        /// </summary>
        public async Task<Result<PreparacionDiariaDto>> Handle(
            ObtenerPreparacionDiariaPorIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("🔍 Obteniendo preparación diaria por ID: {PreparacionId}", request.Id);

            try
            {
                // Buscar la preparación diaria
                var preparacion = await _context.PreparacionesDiarias
                    .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

                if (preparacion == null)
                {
                    _logger.LogWarning("⚠️ Preparación diaria no encontrada: {PreparacionId}", request.Id);
                    return Result.Failure<PreparacionDiariaDto>("Preparación diaria no encontrada");
                }

                // Mapear a DTO
                var preparacionDto = _mapper.Map<PreparacionDiariaDto>(preparacion);

                _logger.LogInformation("✅ Preparación diaria obtenida exitosamente: {PreparacionId}", preparacion.Id);

                return Result.Success(preparacionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparación diaria por ID: {PreparacionId}", request.Id);
                return Result.Failure<PreparacionDiariaDto>($"Error al obtener preparación diaria: {ex.Message}");
            }
        }
    }
} 