using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetaPorId;

/// <summary>
/// Handler para obtener una receta específica por ID
/// </summary>
public class ObtenerRecetaPorIdQueryHandler : IRequestHandler<ObtenerRecetaPorIdQuery, Result<RecetaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerRecetaPorIdQueryHandler> _logger;

    public ObtenerRecetaPorIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerRecetaPorIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<RecetaDto>> Handle(
        ObtenerRecetaPorIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo receta por ID: {Id}", request.Id);

            var query = _context.Recetas.AsQueryable();

            // Filtrar solo recetas no eliminadas
            var receta = await query
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.RecetaEliminada, cancellationToken);

            if (receta == null)
            {
                _logger.LogWarning("⚠️ Receta no encontrada con ID: {Id}", request.Id);
                return Result.Failure<RecetaDto>("Receta no encontrada");
            }

            // Mapear a DTO y poblar NombreProducto manualmente
            var recetaDto = _mapper.Map<RecetaDto>(receta);
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == recetaDto.ProductoId, cancellationToken);
            recetaDto.NombreProducto = producto?.Nombre ?? string.Empty;

            _logger.LogInformation("✅ Receta obtenida exitosamente: {Id}", request.Id);

            return Result.Success(recetaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener receta con ID: {Id}", request.Id);
            return Result.Failure<RecetaDto>("Error al obtener la receta");
        }
    }
} 