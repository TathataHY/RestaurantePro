using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para filtros de inventario
/// </summary>
public class InventarioFiltrosDto
{
    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Categoría del ingrediente
    /// </summary>
    public string? Categoria { get; set; }

    /// <summary>
    /// Proveedor del ingrediente
    /// </summary>
    public string? Proveedor { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool EstaActivo { get; set; } = true;

    /// <summary>
    /// Indica si está bajo stock
    /// </summary>
    public bool StockBajo { get; set; }

    /// <summary>
    /// Indica si está próximo a vencer
    /// </summary>
    public bool VencimientoProximo { get; set; }

    /// <summary>
    /// Fecha de vencimiento desde
    /// </summary>
    public DateTime? FechaVencimientoDesde { get; set; }

    /// <summary>
    /// Fecha de vencimiento hasta
    /// </summary>
    public DateTime? FechaVencimientoHasta { get; set; }

    /// <summary>
    /// Stock mínimo desde
    /// </summary>
    public decimal? StockMinimoDesde { get; set; }

    /// <summary>
    /// Stock mínimo hasta
    /// </summary>
    public decimal? StockMinimoHasta { get; set; }

    /// <summary>
    /// Costo desde
    /// </summary>
    public decimal? CostoDesde { get; set; }

    /// <summary>
    /// Costo hasta
    /// </summary>
    public decimal? CostoHasta { get; set; }
}
