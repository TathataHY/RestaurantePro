using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReportePromociones;

public class ObtenerReportePromocionesQuery : IRequest<Result<ReportePromocionesDto>>
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool SoloActivas { get; set; } = true;
} 