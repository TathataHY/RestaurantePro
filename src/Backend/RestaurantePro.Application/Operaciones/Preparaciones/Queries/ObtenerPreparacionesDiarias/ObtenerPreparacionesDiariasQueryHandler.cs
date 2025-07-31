using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesDiarias
{
    /// <summary>
    /// Manejador para la query de obtener preparaciones diarias
    /// </summary>
    public class ObtenerPreparacionesDiariasQueryHandler : IRequestHandler<ObtenerPreparacionesDiariasQuery, Result<List<PreparacionDiariaDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ObtenerPreparacionesDiariasQueryHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObtenerPreparacionesDiariasQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ObtenerPreparacionesDiariasQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja la query para obtener todas las preparaciones diarias
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> Handle(
            ObtenerPreparacionesDiariasQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("📋 Obteniendo todas las preparaciones diarias");

            try
            {
                // Obtener todas las preparaciones diarias
                var preparaciones = await _context.PreparacionesDiarias
                    .OrderByDescending(p => p.FechaPreparacion)
                    .ToListAsync(cancellationToken);

                // Mapear a DTOs
                var preparacionesDto = _mapper.Map<List<PreparacionDiariaDto>>(preparaciones);

                _logger.LogInformation("✅ Preparaciones diarias obtenidas exitosamente: {Cantidad} preparaciones", preparacionesDto.Count);

                return Result.Success(preparacionesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener preparaciones diarias");
                return Result.Failure<List<PreparacionDiariaDto>>($"Error al obtener preparaciones diarias: {ex.Message}");
            }
        }
    }
} 