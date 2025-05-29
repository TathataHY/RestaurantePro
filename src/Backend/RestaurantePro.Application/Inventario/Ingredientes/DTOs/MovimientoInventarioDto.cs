namespace RestaurantePro.Application.Inventario.Ingredientes.DTOs;

/// <summary>
/// DTO para MovimientoInventario
/// Representa los movimientos de entrada y salida de stock
/// </summary>
public class MovimientoInventarioDto
{
    /// <summary>
    /// ID único del movimiento
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del ingrediente afectado
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Tipo de movimiento (Ingreso, Egreso, Ajuste)
    /// </summary>
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del movimiento (positiva para ingresos, negativa para egresos)
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Fecha del movimiento
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Stock resultante después del movimiento
    /// </summary>
    public decimal? StockResultante { get; set; }

    /// <summary>
    /// Indica si el movimiento está aplicado
    /// </summary>
    public bool EstaAplicado { get; set; }

    /// <summary>
    /// Usuario que realizó el movimiento
    /// </summary>
    public string? UsuarioMovimiento { get; set; }

    // === PROPIEDADES DE UI ===

    /// <summary>
    /// Color del tipo de movimiento para UI
    /// </summary>
    public string ColorTipo { get; set; } = string.Empty;

    /// <summary>
    /// Icono del tipo de movimiento
    /// </summary>
    public string IconoTipo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha formateada para mostrar
    /// </summary>
    public string FechaTexto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad formateada con signo
    /// </summary>
    public string CantidadTexto { get; set; } = string.Empty;
} 