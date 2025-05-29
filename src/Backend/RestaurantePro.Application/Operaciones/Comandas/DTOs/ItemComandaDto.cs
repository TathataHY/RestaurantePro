namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para los items individuales de una comanda
/// </summary>
public class ItemComandaDto : BaseDto
{
    /// <summary>
    /// ID único del item en la comanda
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string? DescripcionProducto { get; set; }

    /// <summary>
    /// Precio unitario del producto al momento de la orden
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Cantidad del producto en el item
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Subtotal del item (cantidad x precio unitario)
    /// </summary>
    public decimal Subtotal => PrecioUnitario * Cantidad;

    /// <summary>
    /// Observaciones específicas para este item
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Estado del item (pendiente, en preparación, listo)
    /// </summary>
    public EstadoItemComanda Estado { get; set; }

    /// <summary>
    /// Texto representativo del estado del item
    /// </summary>
    public string EstadoTexto => Estado.ToString();

    /// <summary>
    /// Fecha de preparación del item
    /// </summary>
    public DateTime? FechaPreparacion { get; set; }

    /// <summary>
    /// Fecha de entrega del item
    /// </summary>
    public DateTime? FechaEntrega { get; set; }

    /// <summary>
    /// Lista de personalizaciones del item
    /// </summary>
    public List<PersonalizacionDto> Personalizaciones { get; set; } = new();

    /// <summary>
    /// Indica si el item tiene personalizaciones
    /// </summary>
    public bool TienePersonalizaciones { get; set; }

    /// <summary>
    /// Precio adicional por personalizaciones
    /// </summary>
    public decimal PrecioPersonalizaciones { get; set; }

    /// <summary>
    /// Total del item incluyendo personalizaciones
    /// </summary>
    public decimal Total { get; set; }
}

/// <summary>
/// DTO para las personalizaciones de un item
/// </summary>
public class PersonalizacionDto
{
    /// <summary>
    /// ID de la personalización
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de personalización (Extra, Quitar, Sustituir)
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// ID del ingrediente afectado
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad (para extras y sustituciones)
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Precio adicional de la personalización
    /// </summary>
    public decimal PrecioAdicional { get; set; }

    /// <summary>
    /// Detalles adicionales de la personalización
    /// </summary>
    public string? Detalles { get; set; }
} 