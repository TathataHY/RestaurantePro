using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteVentas;

public class ObtenerReporteVentasQuery : IRequest<Result<ReporteVentasDto>>
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Segmento { get; set; }
    public string? TipoVenta { get; set; }
    public bool IncluirCanceladas { get; set; } = false;
} 