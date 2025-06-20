using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionesPaginadas;

/// <summary>
/// Query para obtener preparaciones con paginación
/// </summary>
public class ObtenerPreparacionesPaginadasQuery : IRequest<Result<PaginatedList<PreparacionDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public EstadoPreparacion? Estado { get; set; }
    public Guid? ComandaId { get; set; }
    public Guid? ProductoId { get; set; }
    public Guid? ChefId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
} 