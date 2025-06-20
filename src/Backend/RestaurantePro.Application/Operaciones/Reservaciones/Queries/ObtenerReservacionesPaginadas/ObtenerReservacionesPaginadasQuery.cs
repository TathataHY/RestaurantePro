using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPaginadas;

/// <summary>
/// Query para obtener reservaciones con paginación
/// </summary>
public class ObtenerReservacionesPaginadasQuery : IRequest<Result<PaginatedList<ReservacionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public EstadoReservacion? Estado { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? MesaId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
} 