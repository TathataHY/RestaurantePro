using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.RegistrarMovimiento;

public record RegistrarMovimientoCommand : IRequest<Result<bool>>
{
    public Guid IngredienteId { get; init; }
    public decimal Cantidad { get; init; }
    public TipoMovimientoInventario TipoMovimiento { get; init; }
    public string Motivo { get; init; } = string.Empty;
    public string? Observaciones { get; init; }
    public Guid? UsuarioId { get; init; }
} 