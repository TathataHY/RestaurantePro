using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.Commands.CrearOrdenCompra;

/// <summary>
/// Command para crear una nueva orden de compra
/// </summary>
public class CrearOrdenCompraCommand : IRequest<Result<OrdenCompraDto>>
{
    public Guid ProveedorId { get; set; }
    public DateTime FechaEntregaEsperada { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenCompraItemCommand> Items { get; set; } = new();
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