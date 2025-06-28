using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetasPorProducto;

/// <summary>
/// Handler para obtener las recetas de un producto específico
/// </summary>
public class ObtenerRecetasPorProductoQueryHandler : IRequestHandler<ObtenerRecetasPorProductoQuery, Result<List<RecetaDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerRecetasPorProductoQueryHandler> _logger;

    public ObtenerRecetasPorProductoQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerRecetasPorProductoQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<List<RecetaDto>>> Handle(
        ObtenerRecetasPorProductoQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Obteniendo recetas para producto: {ProductoId}, SoloActivas: {SoloActivas}",
                request.ProductoId, request.SoloActivas);

            // Verificar que el producto existe
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == request.ProductoId && !p.EstaEliminado, cancellationToken);

            if (producto == null)
            {
                _logger.LogWarning("⚠️ Producto no encontrado con ID: {ProductoId}", request.ProductoId);
                return Result.Failure<List<RecetaDto>>("Producto no encontrado");
            }

            var query = _context.Recetas
                .Where(r => r.ProductoId == request.ProductoId && !r.RecetaEliminada);

            // Obtener recetas
            var recetas = await query
                .OrderBy(r => r.FechaCreacion)
                .ToListAsync(cancellationToken);

            // Mapear a DTOs y poblar NombreProducto manualmente
            var recetasDto = _mapper.Map<List<RecetaDto>>(recetas);
            foreach (var recetaDto in recetasDto)
            {
                recetaDto.NombreProducto = producto.Nombre;
            }

            _logger.LogInformation("✅ Se obtuvieron {Count} recetas para el producto {ProductoId}",
                recetasDto.Count, request.ProductoId);

            return Result.Success(recetasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener recetas para producto: {ProductoId}", request.ProductoId);
            return Result.Failure<List<RecetaDto>>("Error al obtener las recetas del producto");
        }
    }
} 