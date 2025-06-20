using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.AprobarOrdenCompra;

/// <summary>
/// Command para aprobar una orden de compra
/// </summary>
public class AprobarOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
} 