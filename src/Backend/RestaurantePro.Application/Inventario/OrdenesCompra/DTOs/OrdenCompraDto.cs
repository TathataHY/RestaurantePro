using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

namespace RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

/// <summary>
/// DTO para representar una orden de compra
/// </summary>
public class OrdenCompraDto
{
    public Guid Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public Guid ProveedorId { get; set; }
    public string NombreProveedor { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaEntregaEsperada { get; set; }
    public DateTime? FechaEntregaReal { get; set; }
    public EstadoOrdenCompra Estado { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public Guid? UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public string? NotasRecepcion { get; set; }
    public List<OrdenCompraItemDto> Items { get; set; } = new();
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// DTO para representar un item de orden de compra
/// </summary>
public class OrdenCompraItemDto
{
    public Guid Id { get; set; }
    public Guid IngredienteId { get; set; }
    public string NombreIngrediente { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Observaciones { get; set; }
}
