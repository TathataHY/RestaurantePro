namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para un producto dentro de una comanda
/// </summary>
public class ComandaProductoDto
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio unitario del producto
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Cantidad solicitada
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio total (PrecioUnitario * Cantidad)
    /// </summary>
    public decimal PrecioTotal => PrecioUnitario * Cantidad;

    /// <summary>
    /// Observaciones específicas del producto
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado del producto en la comanda
    /// </summary>
    public string Estado { get; set; } = "pendiente";

    /// <summary>
    /// Fecha de agregado a la comanda
    /// </summary>
    public DateTime FechaAgregado { get; set; }

    /// <summary>
    /// Categoría del producto
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Descuentos aplicados al producto
    /// </summary>
    public decimal Descuento { get; set; }

    /// <summary>
    /// Precio después de descuentos
    /// </summary>
    public decimal PrecioFinal => PrecioTotal - Descuento;

    /// <summary>
    /// Indica si el producto puede ser editado
    /// </summary>
    public bool PuedeSerEditado => Estado.ToLowerInvariant() switch
    {
        "pendiente" => true,
        "en_preparacion" => false,
        _ => false
    };

    /// <summary>
    /// Color para mostrar en la UI según el estado
    /// </summary>
    public string ColorEstado => Estado.ToLowerInvariant() switch
    {
        "pendiente" => "#FFC107",      // Amarillo
        "en_preparacion" => "#FF9800", // Naranja
        "listo" => "#4CAF50",          // Verde
        "entregado" => "#2196F3",      // Azul
        "cancelado" => "#F44336",      // Rojo
        _ => "#607D8B"                 // Gris azulado por defecto
    };

    /// <summary>
    /// Descripción amigable del estado del producto
    /// </summary>
    public string EstadoDescripcion => Estado.ToLowerInvariant() switch
    {
        "pendiente" => "Pendiente",
        "en_preparacion" => "En Preparación",
        "listo" => "Listo",
        "entregado" => "Entregado",
        "cancelado" => "Cancelado",
        _ => Estado
    };
} 