using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para filtros de movimientos de inventario
/// </summary>
public class MovimientoInventarioFiltrosDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid? IngredienteId { get; set; }

    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public TipoMovimientoInventario? TipoMovimiento { get; set; }

    /// <summary>
    /// Fecha desde
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Usuario responsable
    /// </summary>
    public string? UsuarioResponsable { get; set; }

    /// <summary>
    /// ID de la orden de compra
    /// </summary>
    public Guid? OrdenCompraId { get; set; }

    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid? ComandaId { get; set; }
}
