namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO para crear un nuevo producto
/// Contiene solo las propiedades necesarias para la creación
/// </summary>
public class ProductoCreateDto
{
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
    /// Indica si el producto está activo (por defecto true)
    /// </summary>
    public bool Activo { get; set; } = true;
} 