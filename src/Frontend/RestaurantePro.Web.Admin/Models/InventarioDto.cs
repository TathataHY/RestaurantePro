using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para inventario de ingredientes
/// </summary>
public class InventarioDto
{
    /// <summary>
    /// Identificador único del inventario
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del ingrediente
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Stock actual disponible
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo configurado
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Precio unitario del ingrediente
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Fecha de vencimiento del ingrediente
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Indica si está activo
    /// </summary>
    public bool EstaActivo { get; set; } = true;

    /// <summary>
    /// ID del proveedor
    /// </summary>
    public Guid? ProveedorId { get; set; }

    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string? ProveedorNombre { get; set; }

    /// <summary>
    /// Unidad de medida del ingrediente
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingrediente
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de actualización
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
}