namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO resumido para Comanda - Optimizado para listas y performance
/// Contiene solo los campos esenciales para mostrar en grids y listas
/// </summary>
public class ComandaSummaryDto
{
    /// <summary>
    /// Identificador único de la comanda
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de comanda (legible para humanos)
    /// </summary>
    public string NumeroComanda { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de creación de la comanda
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Estado actual de la comanda (texto legible)
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente (puede ser null para consumo en sala)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// ID de la mesa asignada (puede ser null para delivery)
    /// </summary>
    public Guid? MesaId { get; set; }

    /// <summary>
    /// Número de mesa
    /// </summary>
    public string NumeroMesa { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de servicio (Mesa, Delivery, Para Llevar)
    /// </summary>
    public string TipoServicio { get; set; } = string.Empty;

    /// <summary>
    /// Total de ítems en la comanda
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Cantidad total de productos
    /// </summary>
    public int CantidadTotal { get; set; }

    /// <summary>
    /// Subtotal de la comanda (sin impuestos ni descuentos)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Total de descuentos aplicados
    /// </summary>
    public decimal TotalDescuentos { get; set; }

    /// <summary>
    /// Total de impuestos
    /// </summary>
    public decimal TotalImpuestos { get; set; }

    /// <summary>
    /// Total final de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Observaciones especiales de la comanda
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que tomó la orden
    /// </summary>
    public string TomodaPor { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del mesero responsable de la comanda
    /// </summary>
    public string NombreMesero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha estimada de entrega/preparación
    /// </summary>
    public DateTime? FechaEstimadaEntrega { get; set; }

    /// <summary>
    /// Fecha real de entrega
    /// </summary>
    public DateTime? FechaEntrega { get; set; }

    /// <summary>
    /// Tiempo transcurrido desde creación (en minutos)
    /// </summary>
    public int TiempoTranscurrido => (int)(DateTime.UtcNow - FechaCreacion).TotalMinutes;

    /// <summary>
    /// Tiempo estimado restante (en minutos)
    /// </summary>
    public int? TiempoRestante => FechaEstimadaEntrega.HasValue 
        ? Math.Max(0, (int)(FechaEstimadaEntrega.Value - DateTime.UtcNow).TotalMinutes)
        : null;

    /// <summary>
    /// Prioridad de la comanda (Normal, Alta, Urgente)
    /// </summary>
    public string Prioridad { get; set; } = "Normal";

    /// <summary>
    /// Indicador de retraso en la entrega
    /// </summary>
    public bool EstaRetrasada => FechaEstimadaEntrega.HasValue && DateTime.UtcNow > FechaEstimadaEntrega;

    /// <summary>
    /// Fecha de finalización de la comanda
    /// </summary>
    public DateTime? FechaFinalizacion { get; set; }

    /// <summary>
    /// Duración del servicio en minutos (desde creación hasta finalización)
    /// </summary>
    public int? DuracionServicio { get; set; }

    /// <summary>
    /// Indica si fue un servicio rápido (menos de 30 minutos)
    /// </summary>
    public bool ServicioRapido { get; set; }

    /// <summary>
    /// Indica si es un cliente VIP
    /// </summary>
    public bool EsVip { get; set; }

    /// <summary>
    /// Indica si el cliente es frecuente
    /// </summary>
    public bool ClienteEsFrecuente { get; set; }

    /// <summary>
    /// Eficiencia del servicio (porcentaje basado en tiempo esperado vs real)
    /// </summary>
    public decimal? EficienciaServicio { get; set; }

    /// <summary>
    /// Color del estado para UI (Verde, Amarillo, Rojo)
    /// </summary>
    public string ColorEstado => Estado switch
    {
        "Pendiente" => "yellow",
        "En Preparación" => "blue",
        "Lista" => "green",
        "Entregada" => "gray",
        "Cancelada" => "red",
        _ => "gray"
    };

    /// <summary>
    /// Icono del estado para UI
    /// </summary>
    public string IconoEstado => Estado switch
    {
        "Pendiente" => "clock",
        "En Preparación" => "chef-hat",
        "Lista" => "check-circle",
        "Entregada" => "truck",
        "Cancelada" => "x-circle",
        _ => "help-circle"
    };

    /// <summary>
    /// Resumen de productos principales (primeros 3)
    /// </summary>
    public string ResumenProductos { get; set; } = string.Empty;

    /// <summary>
    /// Indicador de comanda con descuentos aplicados
    /// </summary>
    public bool TieneDescuentos => TotalDescuentos > 0;

    /// <summary>
    /// Indicador de comanda con observaciones especiales
    /// </summary>
    public bool TieneObservaciones => !string.IsNullOrEmpty(Observaciones);

    /// <summary>
    /// Porcentaje de progreso de la comanda (0-100)
    /// </summary>
    public int PorcentajeProgreso => Estado switch
    {
        "Pendiente" => 0,
        "En Preparación" => 50,
        "Lista" => 85,
        "Entregada" => 100,
        "Cancelada" => 0,
        _ => 0
    };
}
