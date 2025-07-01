using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.EnviarOrdenCompra;

/// <summary>
/// Command para enviar una orden de compra al proveedor
/// </summary>
public class EnviarOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
    public Guid? UsuarioId { get; set; }
} 