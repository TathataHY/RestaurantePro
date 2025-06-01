namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para representar items en una división de comanda
/// </summary>
public class DivisionItemsDto
{
    /// <summary>
    /// ID del item de comanda
    /// </summary>
    public Guid ItemComandaId { get; set; }

    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad total del item
    /// </summary>
    public int CantidadTotal { get; set; }

    /// <summary>
    /// Cantidad que va a la nueva comanda
    /// </summary>
    public int CantidadNuevaComanda { get; set; }

    /// <summary>
    /// Cantidad que permanece en la comanda original
    /// </summary>
    public int CantidadComandaOriginal => CantidadTotal - CantidadNuevaComanda;

    /// <summary>
    /// Precio unitario del producto
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Precio total en la nueva comanda
    /// </summary>
    public decimal PrecioTotalNuevaComanda => CantidadNuevaComanda * PrecioUnitario;

    /// <summary>
    /// Precio total que permanece en comanda original
    /// </summary>
    public decimal PrecioTotalComandaOriginal => CantidadComandaOriginal * PrecioUnitario;

    /// <summary>
    /// Observaciones especiales del item
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Indica si el item se puede dividir
    /// </summary>
    public bool EsDivisible { get; set; } = true;

    /// <summary>
    /// Estado del item
    /// </summary>
    public string Estado { get; set; } = string.Empty;
} 