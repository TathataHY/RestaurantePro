namespace RestaurantePro.Application.Operaciones.Comandas.DTOs;

/// <summary>
/// DTO resumido para comandas en listas y reportes
/// Optimizado para consultas masivas y dashboards
/// </summary>
public class ComandaSummaryDto
{
    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número o código de la comanda para display
    /// </summary>
    public string NumeroComanda { get; set; } = string.Empty;

    /// <summary>
    /// ID de la mesa
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public string? NumeroMesa { get; set; }

    /// <summary>
    /// Nombre del mesero responsable
    /// </summary>
    public string? NombreMesero { get; set; }

    /// <summary>
    /// Nombre del cliente (si aplica)
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Estado actual de la comanda
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Estado para mostrar en UI con colores
    /// </summary>
    public string EstadoDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Total de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Cantidad de items en la comanda
    /// </summary>
    public int CantidadItems { get; set; }

    /// <summary>
    /// Tiempo transcurrido desde la creación
    /// </summary>
    public TimeSpan TiempoTranscurrido { get; set; }

    /// <summary>
    /// Tiempo transcurrido en formato texto (ej: "15 min", "1h 30m")
    /// </summary>
    public string TiempoTranscurridoTexto { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la comanda está atrasada según tiempo estimado
    /// </summary>
    public bool EstaAtrasada { get; set; }

    /// <summary>
    /// Prioridad de la comanda (basada en tiempo de espera, cliente VIP, etc.)
    /// </summary>
    public string Prioridad { get; set; } = "Normal";

    /// <summary>
    /// Indica si la comanda tiene observaciones especiales
    /// </summary>
    public bool TieneObservaciones { get; set; }

    /// <summary>
    /// Indica si la comanda tiene descuento aplicado
    /// </summary>
    public bool TieneDescuento { get; set; }

    /// <summary>
    /// Color de estado para UI (verde, amarillo, rojo, etc.)
    /// </summary>
    public string ColorEstado { get; set; } = "#6B7280"; // Default gray
} 