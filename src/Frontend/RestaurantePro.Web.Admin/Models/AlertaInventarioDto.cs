using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para alertas de inventario
/// </summary>
public class AlertaInventarioDto
{
    /// <summary>
    /// Identificador único de la alerta
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string IngredienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de alerta
    /// </summary>
    public TipoAlertaInventario TipoAlerta { get; set; }

    /// <summary>
    /// Título de la alerta
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje de la alerta
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de prioridad de la alerta
    /// </summary>
    public NivelPrioridadAlerta NivelPrioridad { get; set; }

    /// <summary>
    /// Indica si la alerta está activa
    /// </summary>
    public bool EstaActiva { get; set; } = true;

    /// <summary>
    /// Fecha de creación de la alerta
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de resolución de la alerta
    /// </summary>
    public DateTime? FechaResolucion { get; set; }

    /// <summary>
    /// Usuario que resolvió la alerta
    /// </summary>
    public string? UsuarioResolucion { get; set; }

    /// <summary>
    /// Observaciones de la alerta
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Valor actual del stock
    /// </summary>
    public decimal StockActual { get; set; }

    /// <summary>
    /// Stock mínimo configurado
    /// </summary>
    public decimal StockMinimo { get; set; }

    /// <summary>
    /// Fecha de vencimiento (si aplica)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>
    /// Severidad de la alerta
    /// </summary>
    public string Severidad { get; set; } = string.Empty;
}

/// <summary>
/// Tipos de alerta de inventario
/// </summary>
public enum TipoAlertaInventario
{
    /// <summary>
    /// Stock bajo
    /// </summary>
    StockBajo = 0,

    /// <summary>
    /// Sin stock
    /// </summary>
    SinStock = 1,

    /// <summary>
    /// Próximo a vencer
    /// </summary>
    ProximoVencer = 2,

    /// <summary>
    /// Vencido
    /// </summary>
    Vencido = 3,

    /// <summary>
    /// Stock excesivo
    /// </summary>
    StockExcesivo = 4,

    /// <summary>
    /// Sin movimiento
    /// </summary>
    SinMovimiento = 5
}

/// <summary>
/// Niveles de prioridad de alerta
/// </summary>
public enum NivelPrioridadAlerta
{
    /// <summary>
    /// Baja prioridad
    /// </summary>
    Baja = 0,

    /// <summary>
    /// Media prioridad
    /// </summary>
    Media = 1,

    /// <summary>
    /// Alta prioridad
    /// </summary>
    Alta = 2,

    /// <summary>
    /// Crítica
    /// </summary>
    Critica = 3
}
