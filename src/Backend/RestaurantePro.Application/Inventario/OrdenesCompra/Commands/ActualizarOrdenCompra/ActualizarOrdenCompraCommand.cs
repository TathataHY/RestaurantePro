using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;

/// <summary>
/// Command para actualizar una orden de compra existente
/// </summary>
public class ActualizarOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid Id { get; set; }
    public DateTime? FechaEntregaEsperada { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenCompraItemCommand>? Items { get; set; }
    public Guid? UsuarioId { get; set; }
}

/// <summary>
/// Command para representar un item de orden de compra
/// </summary>
public class OrdenCompraItemCommand
{
    public Guid IngredienteId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Observaciones { get; set; }
} 