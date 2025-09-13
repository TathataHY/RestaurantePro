using MediatR;
using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerEstadisticasProductos;

/// <summary>
/// Query para obtener estadísticas de productos
/// </summary>
public record ObtenerEstadisticasProductosQuery : IRequest<Result<EstadisticasProductosDto>>
{
    // Por ahora no necesita parámetros, pero se pueden agregar filtros en el futuro
    // como fechaDesde, fechaHasta, categoriaId, etc.
}
