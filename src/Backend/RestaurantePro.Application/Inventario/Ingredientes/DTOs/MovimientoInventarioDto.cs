using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

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
    /// Nombre del ingrediente afectado
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del movimiento (positiva para ingresos, negativa para egresos)
    /// </summary>
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Tipo de movimiento (Ingreso, Egreso, Ajuste)
    /// </summary>
    public TipoMovimientoInventario TipoMovimiento { get; set; }

    /// <summary>
    /// Texto del tipo de movimiento
    /// </summary>
    public string TipoMovimientoTexto => TipoMovimiento.ToString();

    /// <summary>
    /// Motivo del movimiento
    /// </summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones del movimiento
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// ID del usuario que realizó el movimiento
    /// </summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>
    /// Nombre del usuario que realizó el movimiento
    /// </summary>
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// Fecha de creación del movimiento
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Stock anterior del ingrediente
    /// </summary>
    public decimal StockAnterior { get; set; }

    /// <summary>
    /// Stock posterior del ingrediente
    /// </summary>
    public decimal StockPosterior { get; set; }

    // === PROPIEDADES DE UI ===

    /// <summary>
    /// Cantidad formateada para mostrar
    /// </summary>
    public string CantidadFormateada => $"{Cantidad:N2}";

    /// <summary>
    /// Stock anterior formateado para mostrar
    /// </summary>
    public string StockAnteriorFormateado => $"{StockAnterior:N2}";

    /// <summary>
    /// Stock posterior formateado para mostrar
    /// </summary>
    public string StockPosteriorFormateado => $"{StockPosterior:N2}";

    /// <summary>
    /// Fecha formateada para mostrar
    /// </summary>
    public string FechaFormateada => FechaCreacion.ToString("dd/MM/yyyy HH:mm");

    /// <summary>
    /// Indica si el movimiento es de entrada
    /// </summary>
    public bool EsEntrada => TipoMovimiento == TipoMovimientoInventario.Ingreso || 
                             TipoMovimiento == TipoMovimientoInventario.Entrada ||
                             TipoMovimiento == TipoMovimientoInventario.Incremento;

    /// <summary>
    /// Indica si el movimiento es de salida
    /// </summary>
    public bool EsSalida => TipoMovimiento == TipoMovimientoInventario.Egreso || 
                           TipoMovimiento == TipoMovimientoInventario.Salida ||
                           TipoMovimiento == TipoMovimientoInventario.Decremento;
} 