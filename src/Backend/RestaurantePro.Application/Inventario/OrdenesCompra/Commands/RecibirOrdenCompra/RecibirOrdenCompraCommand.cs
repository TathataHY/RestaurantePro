using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;

/// <summary>
/// Command para recibir una orden de compra
/// </summary>
public class RecibirOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
    public string? NotasRecepcion { get; set; }
    public Guid? UsuarioId { get; set; }
} 