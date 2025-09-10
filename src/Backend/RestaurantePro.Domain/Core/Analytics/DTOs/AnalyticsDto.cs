namespace RestaurantePro.Domain.Core.Analytics.DTOs;

/// <summary>
/// DTO para métricas del día - Domain
/// </summary>
public class MetricasDiaDto
{
    /// <summary>
    /// Fecha de las métricas
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Total de ventas del día
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Número total de comandas
    /// </summary>
    public int TotalComandas { get; set; }

    /// <summary>
    /// Número total de productos vendidos
    /// </summary>
    public int TotalProductosVendidos { get; set; }

    /// <summary>
    /// Promedio de venta por comanda
    /// </summary>
    public decimal PromedioVentaPorComanda => TotalComandas > 0 ? TotalVentas / TotalComandas : 0;

    /// <summary>
    /// Tiempo promedio de preparación en minutos
    /// </summary>
    public int TiempoPromedioPreparacion { get; set; }

    /// <summary>
    /// Porcentaje de ocupación de mesas
    /// </summary>
    public decimal PorcentajeOcupacionMesas { get; set; }

    /// <summary>
    /// Número de clientes atendidos
    /// </summary>
    public int ClientesAtendidos { get; set; }

    /// <summary>
    /// Productos más vendidos del día
    /// </summary>
    public List<TopProductoDto> TopProductos { get; set; } = new();
}

/// <summary>
/// DTO para métricas por rango de fechas - Domain
/// </summary>
public class MetricasRangoDto
{
    /// <summary>
    /// Fecha de inicio del rango
    /// </summary>
    public DateTime FechaDesde { get; set; }

    /// <summary>
    /// Fecha de fin del rango
    /// </summary>
    public DateTime FechaHasta { get; set; }

    /// <summary>
    /// Total de ventas en el rango
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Número total de comandas en el rango
    /// </summary>
    public int TotalComandas { get; set; }

    /// <summary>
    /// Promedio diario de ventas
    /// </summary>
    public decimal PromedioVentasDiarias { get; set; }

    /// <summary>
    /// Promedio diario de comandas
    /// </summary>
    public decimal PromedioComandasDiarias { get; set; }

    /// <summary>
    /// Días en el rango
    /// </summary>
    public int DiasEnRango => (FechaHasta - FechaDesde).Days + 1;

    /// <summary>
    /// Métricas por día
    /// </summary>
    public List<MetricasDiaDto> MetricasPorDia { get; set; } = new();
}

/// <summary>
/// DTO para top de productos - Domain
/// </summary>
public class TopProductoDto
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Posición en el ranking
    /// </summary>
    public int Posicion { get; set; }

    /// <summary>
    /// Nombre del producto
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;

    /// <summary>
    /// Categoría del producto
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad vendida
    /// </summary>
    public int CantidadVendida { get; set; }

    /// <summary>
    /// Total de ventas del producto
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Porcentaje del total de ventas
    /// </summary>
    public decimal PorcentajeTotalVentas { get; set; }

    /// <summary>
    /// Precio promedio del producto
    /// </summary>
    public decimal PrecioPromedio { get; set; }
}

/// <summary>
/// DTO para ocupación de mesas - Domain
/// </summary>
public class OcupacionMesasDto
{
    /// <summary>
    /// Fecha de la métrica
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Total de mesas disponibles
    /// </summary>
    public int TotalMesas { get; set; }

    /// <summary>
    /// Mesas ocupadas
    /// </summary>
    public int MesasOcupadas { get; set; }

    /// <summary>
    /// Mesas disponibles
    /// </summary>
    public int MesasDisponibles { get; set; }

    /// <summary>
    /// Mesas reservadas
    /// </summary>
    public int MesasReservadas { get; set; }

    /// <summary>
    /// Porcentaje de ocupación
    /// </summary>
    public decimal PorcentajeOcupacion => TotalMesas > 0 ? (decimal)MesasOcupadas / TotalMesas * 100 : 0;

    /// <summary>
    /// Promedio de tiempo de ocupación por mesa en minutos
    /// </summary>
    public int TiempoPromedioOcupacion { get; set; }

    /// <summary>
    /// Número de rotaciones de mesas
    /// </summary>
    public int RotacionesMesas { get; set; }
}

/// <summary>
/// DTO para tiempo de preparación - Domain
/// </summary>
public class TiempoPreparacionDto
{
    /// <summary>
    /// Tiempo promedio de preparación en minutos
    /// </summary>
    public int TiempoPromedioMinutos { get; set; }

    /// <summary>
    /// Tiempo mínimo de preparación en minutos
    /// </summary>
    public int TiempoMinimoMinutos { get; set; }

    /// <summary>
    /// Tiempo máximo de preparación en minutos
    /// </summary>
    public int TiempoMaximoMinutos { get; set; }

    /// <summary>
    /// Número total de preparaciones analizadas
    /// </summary>
    public int TotalPreparaciones { get; set; }

    /// <summary>
    /// Preparaciones dentro del tiempo estándar
    /// </summary>
    public int PreparacionesEnTiempo { get; set; }

    /// <summary>
    /// Preparaciones fuera del tiempo estándar
    /// </summary>
    public int PreparacionesFueraTiempo { get; set; }

    /// <summary>
    /// Porcentaje de preparaciones en tiempo
    /// </summary>
    public decimal PorcentajeEnTiempo => TotalPreparaciones > 0 ? (decimal)PreparacionesEnTiempo / TotalPreparaciones * 100 : 0;

    /// <summary>
    /// Tiempo estándar de preparación en minutos
    /// </summary>
    public int TiempoEstandarMinutos { get; set; }
}

/// <summary>
/// DTO para ventas por hora - Domain
/// </summary>
public class VentasHoraDto
{
    /// <summary>
    /// Hora del día (0-23)
    /// </summary>
    public int Hora { get; set; }

    /// <summary>
    /// Total de ventas en esa hora
    /// </summary>
    public decimal TotalVentas { get; set; }

    /// <summary>
    /// Número de comandas en esa hora
    /// </summary>
    public int NumeroComandas { get; set; }

    /// <summary>
    /// Promedio de venta por comanda en esa hora
    /// </summary>
    public decimal PromedioVentaPorComanda => NumeroComandas > 0 ? TotalVentas / NumeroComandas : 0;

    /// <summary>
    /// Porcentaje del total de ventas del día
    /// </summary>
    public decimal PorcentajeTotalVentas { get; set; }

    /// <summary>
    /// Etiqueta de hora formateada
    /// </summary>
    public string HoraFormateada => $"{Hora:00}:00";
} 