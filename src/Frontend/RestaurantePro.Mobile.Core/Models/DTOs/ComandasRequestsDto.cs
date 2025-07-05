namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Request para crear una nueva comanda
/// </summary>
public class CrearComandaRequest
{
    /// <summary>
    /// ID de la mesa asociada
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Nombre del cliente (opcional)
    /// </summary>
    public string? ClienteNombre { get; set; }

    /// <summary>
    /// Observaciones iniciales
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Productos iniciales (opcional)
    /// </summary>
    public List<ComandaProductoRequest> Productos { get; set; } = new List<ComandaProductoRequest>();
}

/// <summary>
/// Request para agregar o actualizar un producto en la comanda
/// </summary>
public class ComandaProductoRequest
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad solicitada
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario (opcional, se tomará del catálogo si no se especifica)
    /// </summary>
    public decimal? PrecioUnitario { get; set; }

    /// <summary>
    /// Observaciones específicas del producto
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Descuento específico para este producto (opcional)
    /// </summary>
    public decimal? Descuento { get; set; }
}

/// <summary>
/// DTO para estadísticas de comandas
/// </summary>
public class EstadisticasComandasDto
{
    /// <summary>
    /// Total de comandas activas
    /// </summary>
    public int TotalComandasActivas { get; set; }

    /// <summary>
    /// Comandas pendientes
    /// </summary>
    public int ComandasPendientes { get; set; }

    /// <summary>
    /// Comandas en preparación
    /// </summary>
    public int ComandasEnPreparacion { get; set; }

    /// <summary>
    /// Comandas listas para entregar
    /// </summary>
    public int ComandasListas { get; set; }

    /// <summary>
    /// Total de comandas completadas hoy
    /// </summary>
    public int ComandasCompletadasHoy { get; set; }

    /// <summary>
    /// Total de comandas canceladas hoy
    /// </summary>
    public int ComandasCanceladasHoy { get; set; }

    /// <summary>
    /// Tiempo promedio de preparación (en minutos)
    /// </summary>
    public double TiempoPromedioPreparacion { get; set; }

    /// <summary>
    /// Valor total de ventas del día
    /// </summary>
    public decimal VentasTotalDia { get; set; }

    /// <summary>
    /// Valor promedio por comanda
    /// </summary>
    public decimal ValorPromedioPorComanda { get; set; }

    /// <summary>
    /// Producto más vendido hoy
    /// </summary>
    public string ProductoMasVendido { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad del producto más vendido
    /// </summary>
    public int CantidadProductoMasVendido { get; set; }

    /// <summary>
    /// Mesa más activa (con más comandas)
    /// </summary>
    public string MesaMasActiva { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de comandas en la mesa más activa
    /// </summary>
    public int ComandasEnMesaMasActiva { get; set; }
} 