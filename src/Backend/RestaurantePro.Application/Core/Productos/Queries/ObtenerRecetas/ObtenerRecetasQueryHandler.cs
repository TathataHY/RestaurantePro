using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Entities;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetas;

/// <summary>
/// Handler para obtener todas las recetas disponibles
/// </summary>
public class ObtenerRecetasQueryHandler : IRequestHandler<ObtenerRecetasQuery, Result<List<RecetaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerRecetasQueryHandler> _logger;

    public ObtenerRecetasQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerRecetasQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<RecetaDto>>> Handle(
        ObtenerRecetasQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo recetas con filtros: SoloActivas={SoloActivas}, ProductoId={ProductoId}",
                request.SoloActivas, request.ProductoId);

            var query = _context.Recetas.AsQueryable();

            // Filtrar solo recetas no eliminadas
            query = query.Where(r => !r.RecetaEliminada);

            if (request.ProductoId.HasValue)
            {
                query = query.Where(r => r.ProductoId == request.ProductoId.Value);
            }

            // Obtener recetas
            var recetas = await query
                .OrderBy(r => r.FechaCreacion)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs y poblar NombreProducto manualmente
            var recetasDto = _mapper.Map<List<RecetaDto>>(recetas);
            foreach (var recetaDto in recetasDto)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == recetaDto.ProductoId, cancellationToken);
                recetaDto.NombreProducto = producto?.Nombre ?? string.Empty;
            }

            _logger.LogInformation("✅ Se obtuvieron {Count} recetas", recetasDto.Count);

            return Result.Success(recetasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener recetas");
            return Result.Failure<List<RecetaDto>>("Error al obtener las recetas");
        }
    }
} 