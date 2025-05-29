namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO completo para la entidad Comanda
/// Incluye toda la información de la comanda con propiedades calculadas
/// </summary>
public class ComandaDto : BaseDto
{
    /// <summary>
    /// ID de la mesa asociada a la comanda
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa (si está disponible)
    /// </summary>
    public string? NumeroMesa { get; set; }

    /// <summary>
    /// ID del mesero responsable de la comanda
    /// </summary>
    public Guid MeseroId { get; set; }

    /// <summary>
    /// Nombre del mesero (si está disponible)
    /// </summary>
    public string? NombreMesero { get; set; }

    /// <summary>
    /// ID del cliente asociado (opcional)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente (si está disponible)
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Estado actual de la comanda
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la comanda como enum string para facilitar el frontend
    /// </summary>
    public string EstadoTexto { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Lista de items de la comanda
    /// </summary>
    public List<ItemComandaDto> Items { get; set; } = new();

    /// <summary>
    /// Subtotal de la comanda (sin impuestos ni descuentos)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Impuestos aplicados
    /// </summary>
    public decimal Impuestos { get; set; }

    /// <summary>
    /// Descuento por fidelización aplicado
    /// </summary>
    public decimal? DescuentoFidelizacion { get; set; }

    /// <summary>
    /// Total final de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Cantidad total de items en la comanda
    /// </summary>
    public int CantidadItems { get; set; }

    /// <summary>
    /// Indica si la comanda tiene descuento de fidelización
    /// </summary>
    public bool TieneDescuentoFidelizacion { get; set; }

    /// <summary>
    /// Indica si la comanda se puede modificar (agregar/quitar productos)
    /// </summary>
    public bool PuedeModificar { get; set; }

    /// <summary>
    /// Indica si la comanda se puede cancelar
    /// </summary>
    public bool PuedeCancelar { get; set; }

    /// <summary>
    /// Tiempo transcurrido desde la creación (para métricas)
    /// </summary>
    public TimeSpan TiempoTranscurrido { get; set; }

    /// <summary>
    /// Tiempo promedio estimado de preparación
    /// </summary>
    public TimeSpan? TiempoEstimadoPreparacion { get; set; }
} 