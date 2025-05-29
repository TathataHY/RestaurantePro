namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO resumido para Producto - Optimizado para listas y performance
/// Contiene solo los campos esenciales para mostrar en grids y listas
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
    /// Precio del producto
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// ID de la categoría del producto
    /// </summary>
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Nombre de la categoría (texto legible)
    /// </summary>
    public string CategoriaNombre { get; set; } = string.Empty;

    /// <summary>
    /// Estado activo del producto
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Usuario que creó el producto
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Descripción resumida (primeros 100 caracteres)
    /// </summary>
    public string DescripcionCorta { get; set; } = string.Empty;

    /// <summary>
    /// Indica si está disponible actualmente
    /// Derivado de Activo y otros factores de negocio
    /// </summary>
    public bool Disponible { get; set; }

    /// <summary>
    /// Número total de ingredientes de la receta
    /// </summary>
    public int TotalIngredientes { get; set; }

    /// <summary>
    /// Costo estimado del producto
    /// </summary>
    public decimal? CostoEstimado { get; set; }

    /// <summary>
    /// Margen de ganancia estimado
    /// </summary>
    public decimal? MargenGanancia => CostoEstimado.HasValue ? Precio - CostoEstimado.Value : null;

    /// <summary>
    /// Porcentaje de margen de ganancia
    /// </summary>
    public decimal? PorcentajeMargen => CostoEstimado.HasValue && CostoEstimado.Value > 0 
        ? Math.Round(((Precio - CostoEstimado.Value) / CostoEstimado.Value) * 100, 2) 
        : null;
}