using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.AsociarProveedor;

public record AsociarProveedorCommand : IRequest<Result<bool>>
{
    public Guid IngredienteId { get; init; }
    public Guid ProveedorId { get; init; }
    public bool EsProveedorPrincipal { get; init; } = false;
    public decimal? PrecioUnitario { get; init; }
    public string? Observaciones { get; init; }
} 