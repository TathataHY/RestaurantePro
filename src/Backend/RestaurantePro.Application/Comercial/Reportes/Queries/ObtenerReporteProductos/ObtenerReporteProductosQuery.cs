using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteProductos;

public class ObtenerReporteProductosQuery : IRequest<Result<ReporteProductosDto>>
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TopProductos { get; set; } = 10;
    public string? Categoria { get; set; }
} 