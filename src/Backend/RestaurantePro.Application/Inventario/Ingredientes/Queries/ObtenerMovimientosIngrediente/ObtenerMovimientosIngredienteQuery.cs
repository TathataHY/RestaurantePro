using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerMovimientosIngrediente;

public record ObtenerMovimientosIngredienteQuery : IRequest<Result<List<MovimientoInventarioDto>>>
{
    public Guid IngredienteId { get; init; }
    public DateTime? FechaDesde { get; init; }
    public DateTime? FechaHasta { get; init; }
    public int? Limite { get; init; } = 100;
} 