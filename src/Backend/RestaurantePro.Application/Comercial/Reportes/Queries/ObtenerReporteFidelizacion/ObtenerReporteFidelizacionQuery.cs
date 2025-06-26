using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteFidelizacion;

public class ObtenerReporteFidelizacionQuery : IRequest<Result<ReporteFidelizacionDto>>
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool SoloActivas { get; set; } = true;
} 