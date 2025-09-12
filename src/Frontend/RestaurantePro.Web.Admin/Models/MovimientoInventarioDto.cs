using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para movimientos de inventario
/// </summary>
public class MovimientoInventarioDto
{
    /// <summary>
    /// Identificador único del movimiento
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
    /// Tipo de movimiento
    /// </summary>
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    /// <summary>
    /// Cantidad del movimiento
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del movimiento
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Total del movimiento
    /// </summary>
    public decimal Total => Cantidad * PrecioUnitario;

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Usuario responsable del movimiento
    /// </summary>
    public string UsuarioResponsable { get; set; } = string.Empty;

    /// <summary>
    /// ID de la orden de compra (si aplica)
    /// </summary>
    public Guid? OrdenCompraId { get; set; }

    /// <summary>
    /// ID de la comanda (si aplica)
    /// </summary>
    public Guid? ComandaId { get; set; }

    /// <summary>
    /// Observaciones del movimiento
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Tipos de movimiento de inventario
/// </summary>
public enum TipoMovimientoInventario
{
    /// <summary>
    /// Entrada de inventario
    /// </summary>
    Entrada = 0,

    /// <summary>
    /// Salida de inventario
    /// </summary>
    Salida = 1,

    /// <summary>
    /// Ajuste de inventario
    /// </summary>
    Ajuste = 2,

    /// <summary>
    /// Transferencia de inventario
    /// </summary>
    Transferencia = 3,

    /// <summary>
    /// Pérdida de inventario
    /// </summary>
    Perdida = 4,

    /// <summary>
    /// Vencimiento de inventario
    /// </summary>
    Vencimiento = 5
}
