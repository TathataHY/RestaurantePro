using MediatR;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerEstadisticasProductos;

/// <summary>
/// Handler para obtener estadísticas de productos
/// </summary>
public class ObtenerEstadisticasProductosHandler : IRequestHandler<ObtenerEstadisticasProductosQuery, Result<EstadisticasProductosDto>>
{
    private readonly IProductoRepository _productoRepository;
    private readonly ILogger<ObtenerEstadisticasProductosHandler> _logger;

    public ObtenerEstadisticasProductosHandler(
        IProductoRepository productoRepository,
        ILogger<ObtenerEstadisticasProductosHandler> logger)
    {
        _productoRepository = productoRepository;
        _logger = logger;
    }

    public async Task<Result<EstadisticasProductosDto>> Handle(ObtenerEstadisticasProductosQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("📊 Obteniendo estadísticas de productos");

        try
        {
            // Obtener todos los productos para calcular estadísticas
            var todosLosProductos = await _productoRepository.ObtenerTodosAsync();
            
            if (!todosLosProductos.Any())
            {
                _logger.LogInformation("⚠️ No se encontraron productos para calcular estadísticas");
                return Result<EstadisticasProductosDto>.Success(new EstadisticasProductosDto());
            }

            _logger.LogInformation("📊 Calculando estadísticas para {Count} productos", todosLosProductos.Count());

            // Calcular estadísticas
            var estadisticas = CalcularEstadisticas(todosLosProductos.ToList());

            _logger.LogInformation("✅ Estadísticas calculadas: {TotalProductos} productos totales", estadisticas.TotalProductos);
            
            return Result<EstadisticasProductosDto>.Success(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener estadísticas de productos");
            return Result.Failure<EstadisticasProductosDto>("Error al obtener estadísticas de productos");
        }
    }

    private EstadisticasProductosDto CalcularEstadisticas(List<Domain.Core.Productos.Entities.Producto> productos)
    {
        var stats = new EstadisticasProductosDto
        {
            TotalProductos = productos.Count,
            ProductosActivos = productos.Count(p => p.EstaActivo),
            ProductosInactivos = productos.Count(p => !p.EstaActivo)
        };

        // Calcular estadísticas de precios
        if (productos.Any())
        {
            stats.PrecioPromedio = productos.Average(p => p.Precio?.Valor ?? 0);
            stats.PrecioMinimo = productos.Min(p => p.Precio?.Valor ?? 0);
            stats.PrecioMaximo = productos.Max(p => p.Precio?.Valor ?? 0);
        }

        // Distribución por categoría
        stats.ProductosPorCategoria = productos
            .GroupBy(p => p.CategoriaNombre ?? "Sin categoría")
            .Select(g => new ProductosPorCategoriaDto
            {
                Nombre = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ToList();

        // Distribución por popularidad
        stats.ProductosPorPopularidad = productos
            .GroupBy(p => p.Popularidad)
            .Select(g => new ProductosPorPopularidadDto
            {
                Nivel = g.Key,
                Cantidad = g.Count()
            })
            .OrderBy(x => x.Nivel)
            .ToList();

        // Productos recientes (últimos 6)
        stats.ProductosRecientes = productos
            .OrderByDescending(p => p.FechaCreacion)
            .Take(6)
            .Select(p => new ProductoDto
            {
                Id = p.Id,
                CategoriaId = p.CategoriaId,
                CategoriaNombre = p.CategoriaNombre ?? "Sin categoría",
                Nombre = p.Nombre ?? "Sin nombre",
                Descripcion = p.Descripcion ?? "",
                Precio = p.Precio?.Valor ?? 0,
                Popularidad = p.Popularidad,
                Activo = p.EstaActivo,
                ImagenUrl = p.ImagenUrl,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaActualizacion,
                CreadoPor = p.CreatedBy ?? "Sistema",
                ModificadoPor = p.LastModifiedBy ?? "Sistema"
            })
            .ToList();

        return stats;
    }
}