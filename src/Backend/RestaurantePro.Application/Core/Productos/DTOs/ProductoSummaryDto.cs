namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO resumido para productos
/// Usado en listas, búsquedas y cuando no se necesitan todos los detalles
/// </summary>
public class ProductoSummaryDto
{
    /// <summary>
    /// Identificador único del producto
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción corta del producto
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string CategoriaNombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el producto está activo
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Nivel de popularidad (0-10)
    /// </summary>
    public int Popularidad { get; set; }
} 