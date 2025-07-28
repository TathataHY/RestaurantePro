namespace RestaurantePro.Application.Core.Productos.DTOs;

/// <summary>
/// DTO para categoría de productos - Backend
/// </summary>
public class CategoriaProductoDto
{
    /// <summary>
    /// Identificador único de la categoría
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Color asociado a la categoría para UI
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Icono asociado a la categoría
    /// </summary>
    public string? Icono { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int Orden { get; set; }

    /// <summary>
    /// Indica si la categoría está activa
    /// </summary>
    public bool Activa { get; set; }

    /// <summary>
    /// Cantidad de productos en esta categoría
    /// </summary>
    public int CantidadProductos { get; set; }

    /// <summary>
    /// Cantidad de productos disponibles en esta categoría
    /// </summary>
    public int ProductosDisponibles { get; set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
} 