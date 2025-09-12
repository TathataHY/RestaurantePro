using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para órdenes de compra
/// </summary>
public class OrdenCompraDto
{
    /// <summary>
    /// Identificador único de la orden
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de orden
    /// </summary>
    public string NumeroOrden { get; set; } = string.Empty;

    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string ProveedorNombre { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la orden
    /// </summary>
    public EstadoOrdenCompra Estado { get; set; }

    /// <summary>
    /// Fecha de la orden
    /// </summary>
    public DateTime FechaOrden { get; set; }

    /// <summary>
    /// Fecha de entrega esperada
    /// </summary>
    public DateTime? FechaEntregaEsperada { get; set; }

    /// <summary>
    /// Fecha de entrega real
    /// </summary>
    public DateTime? FechaEntregaReal { get; set; }

    /// <summary>
    /// Subtotal de la orden
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Impuestos de la orden
    /// </summary>
    public decimal Impuestos { get; set; }

    /// <summary>
    /// Total de la orden
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Usuario responsable
    /// </summary>
    public string UsuarioResponsable { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones de la orden
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de actualización
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }

    /// <summary>
    /// Items de la orden
    /// </summary>
    public List<OrdenCompraItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO para items de orden de compra
/// </summary>
public class OrdenCompraItemDto
{
    /// <summary>
    /// Identificador único del item
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string IngredienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad solicitada
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Cantidad recibida
    /// </summary>
    public decimal CantidadRecibida { get; set; }

    /// <summary>
    /// Precio unitario
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Total del item
    /// </summary>
    public decimal Total => Cantidad * PrecioUnitario;

    /// <summary>
    /// Observaciones del item
    /// </summary>
    public string? Observaciones { get; set; }
}

/// <summary>
/// Estados de orden de compra
/// </summary>
public enum EstadoOrdenCompra
{
    /// <summary>
    /// Borrador
    /// </summary>
    Borrador = 0,

    /// <summary>
    /// Enviada
    /// </summary>
    Enviada = 1,

    /// <summary>
    /// Confirmada
    /// </summary>
    Confirmada = 2,

    /// <summary>
    /// En tránsito
    /// </summary>
    EnTransito = 3,

    /// <summary>
    /// Entregada
    /// </summary>
    Entregada = 4,

    /// <summary>
    /// Cancelada
    /// </summary>
    Cancelada = 5
}
