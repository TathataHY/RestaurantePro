namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO principal para transferencia de datos de productos
/// Usado por todos los comandos y queries del contexto Productos
/// </summary>
public class ProductoDto
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
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto
    /// </summary>
    public decimal Precio { get; set; }

    /// <summary>
    /// ID de la categoría a la que pertenece
    /// </summary>
    public Guid CategoriaId { get; set; }

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

    /// <summary>
    /// URL de la imagen del producto
    /// </summary>
    public string? ImagenUrl { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de última modificación
    /// </summary>
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Usuario que creó el producto
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que modificó el producto por última vez
    /// </summary>
    public string? ModificadoPor { get; set; }
}