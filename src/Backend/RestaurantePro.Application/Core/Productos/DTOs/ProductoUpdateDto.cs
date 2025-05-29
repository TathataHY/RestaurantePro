namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO para actualizar un producto existente
/// Incluye el ID para identificar qué producto actualizar
/// </summary>
public class ProductoUpdateDto
{
    /// <summary>
    /// Identificador del producto a actualizar
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
    /// Indica si el producto está activo
    /// </summary>
    public bool Activo { get; set; } = true;
} 