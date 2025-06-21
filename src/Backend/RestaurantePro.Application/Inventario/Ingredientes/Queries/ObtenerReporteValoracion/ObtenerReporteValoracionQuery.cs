using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerReporteValoracion;

public record ObtenerReporteValoracionQuery : IRequest<Result<ReporteValoracionDto>>
{
    public DateTime? FechaDesde { get; init; }
    public DateTime? FechaHasta { get; init; }
    public bool IncluirIngredientesSinStock { get; init; } = false;
} 