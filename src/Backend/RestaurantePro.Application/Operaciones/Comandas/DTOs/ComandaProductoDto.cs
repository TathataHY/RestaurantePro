namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para un producto dentro de una comanda
/// </summary>
public class ComandaProductoDto
{
    /// <summary>
    /// ID del item en la comanda (clave del ItemComanda)
    /// </summary>
    public Guid ItemId { get; set; }

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
}
