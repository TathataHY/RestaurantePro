namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO para crear un item de comanda
/// </summary>
public class CrearItemComandaDto
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad del producto
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto
    /// </summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Observaciones especiales para el item
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Alias para Observaciones (compatibilidad con tests)
    /// </summary>
    public string? ObservacionesEspeciales 
    { 
        get => Observaciones; 
        set => Observaciones = value; 
    }

    /// <summary>
    /// Lista de personalizaciones del item
    /// </summary>
    public List<CrearPersonalizacionItemDto> Personalizaciones { get; set; } = new();

    /// <summary>
    /// Prioridad de preparación (1 = Alta, 5 = Baja)
    /// </summary>
    public int PrioridadPreparacion { get; set; } = 3;

    /// <summary>
    /// Indica si es un item promocional
    /// </summary>
    public bool EsPromocional { get; set; }

    /// <summary>
    /// ID de la promoción aplicada (si aplica)
    /// </summary>
    public Guid? PromocionId { get; set; }

    /// <summary>
    /// Descuento aplicado al item
    /// </summary>
    public decimal DescuentoAplicado { get; set; }

    /// <summary>
    /// Indica si requiere confirmación del chef
    /// </summary>
    public bool RequiereConfirmacionChef { get; set; }

    /// <summary>
    /// Tiempo estimado de preparación en minutos
    /// </summary>
    public int TiempoPreparacionEstimado { get; set; }
} 