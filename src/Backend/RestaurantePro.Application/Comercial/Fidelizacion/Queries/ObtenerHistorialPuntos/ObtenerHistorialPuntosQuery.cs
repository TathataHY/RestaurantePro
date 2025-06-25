using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Queries.ObtenerHistorialPuntos;

/// <summary>
/// Query para obtener el historial de puntos de una tarjeta de fidelización
/// </summary>
public class ObtenerHistorialPuntosQuery : IRequest<Result<List<HistorialPuntosDto>>>
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
} 