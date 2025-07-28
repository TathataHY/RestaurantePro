namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para métricas operativas del día
/// </summary>
public class MetricasDiaDto
{
    public DateTime Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public int TotalProductosVendidos { get; set; }
    public int TiempoPromedioPreparacion { get; set; }
    public decimal PorcentajeOcupacionMesas { get; set; }
    public int ClientesAtendidos { get; set; }
    public List<TopProductoDto> TopProductos { get; set; } = new();
}

/// <summary>
/// DTO para métricas operativas por rango de fechas
/// </summary>
public class MetricasRangoDto
{
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public int TotalProductosVendidos { get; set; }
    public int TiempoPromedioPreparacion { get; set; }
    public decimal PorcentajeOcupacionMesas { get; set; }
    public int ClientesAtendidos { get; set; }
    public List<TopProductoDto> TopProductos { get; set; } = new();
    public List<VentasHoraDto> VentasPorHora { get; set; } = new();
}

/// <summary>
/// DTO para productos más vendidos
/// </summary>
public class TopProductoDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PorcentajeTotalVentas { get; set; }
    public decimal PrecioPromedio { get; set; }
}

/// <summary>
/// DTO para métricas de ocupación de mesas
/// </summary>
public class OcupacionMesasDto
{
    public DateTime Fecha { get; set; }
    public int TotalMesas { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasLibres { get; set; }
    public int MesasReservadas { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
    public decimal TiempoPromedioOcupacion { get; set; }
    public List<OcupacionMesaDetalleDto> DetalleMesas { get; set; } = new();
}

/// <summary>
/// DTO para detalle de ocupación por mesa
/// </summary>
public class OcupacionMesaDetalleDto
{
    public Guid MesaId { get; set; }
    public string NumeroMesa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? HoraOcupacion { get; set; }
    public decimal? TiempoOcupacion { get; set; }
    public int? NumeroComandas { get; set; }
}

/// <summary>
/// DTO para métricas de tiempo de preparación
/// </summary>
public class TiempoPreparacionDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int TiempoPromedioMinutos { get; set; }
    public int TiempoMinimoMinutos { get; set; }
    public int TiempoMaximoMinutos { get; set; }
    public int TotalPreparaciones { get; set; }
    public List<TiempoPreparacionCategoriaDto> PorCategoria { get; set; } = new();
}

/// <summary>
/// DTO para tiempo de preparación por categoría
/// </summary>
public class TiempoPreparacionCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public int TiempoPromedioMinutos { get; set; }
    public int TotalPreparaciones { get; set; }
}

/// <summary>
/// DTO para ventas por hora
/// </summary>
public class VentasHoraDto
{
    public int Hora { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public int TotalProductos { get; set; }
    public decimal PromedioPorComanda { get; set; }
} 