using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;

/// <summary>
/// Command para rechazar una orden de compra
/// </summary>
public class RechazarOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
    public string MotivoRechazo { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
} 