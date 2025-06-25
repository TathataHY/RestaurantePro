using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerEstadisticasTarjeta;

/// <summary>
/// Query para obtener las estadísticas de una tarjeta de fidelización
/// </summary>
public class ObtenerEstadisticasTarjetaQuery : IRequest<Result<EstadisticasTarjetaDto>>
{
    public Guid TarjetaFidelizacionId { get; set; }
} 