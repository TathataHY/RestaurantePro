using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteClientes;

public class ObtenerReporteClientesQuery : IRequest<Result<ReporteClientesDto>>
{
    public bool SoloActivos { get; set; } = true;
    public string? Segmento { get; set; }
    public DateTime? FechaRegistroDesde { get; set; }
    public DateTime? FechaRegistroHasta { get; set; }
} 