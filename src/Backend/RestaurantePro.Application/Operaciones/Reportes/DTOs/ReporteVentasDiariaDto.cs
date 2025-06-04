namespace RestaurantePro.Application.Operaciones.Reportes.DTOs;

/// <summary>
/// 📊 DTO principal para el reporte de ventas diarias
/// </summary>
public class ReporteVentasDiariaDto
{
    public DateTime FechaReporte { get; set; }
    public NivelDetalle NivelDetalle { get; set; }
    public MetricasBasicasDto MetricasBasicas { get; set; } = new();
    public List<AnalisisMesaDto>? AnalisisPorMesa { get; set; }
    public List<AnalisisMeseroDto>? AnalisisPorMesero { get; set; }
    public List<AnalisisProductoDto>? AnalisisProductos { get; set; }
    public List<DistribucionHorariaDto> DistribucionHoraria { get; set; } = new();
    public ComparativoPeriodoDto? ComparativoPeriodoAnterior { get; set; }
    public List<TendenciaDiariaDto>? TendenciasSemana { get; set; }
    public DateTime FechaGeneracion { get; set; }
}

/// <summary>
/// 📈 Métricas básicas del día
/// </summary>
public class MetricasBasicasDto
{
    public int TotalComandas { get; set; }
    public decimal MontoTotalVentas { get; set; }
    public decimal PromedioVentaPorComanda { get; set; }
    public TimeSpan HoraPico { get; set; }
    public string ProductoMasVendido { get; set; } = string.Empty;
}

/// <summary>
/// 🪑 Análisis por mesa
/// </summary>
public class AnalisisMesaDto
{
    public Guid MesaId { get; set; }
    public int NumeroMesa { get; set; }
    public int TotalComandas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioComanda { get; set; }
    public TimeSpan TiempoPromedioOcupacion { get; set; }
}

/// <summary>
/// 👨‍💼 Análisis por mesero
/// </summary>
public class AnalisisMeseroDto
{
    public Guid MeseroId { get; set; }
    public string NombreMesero { get; set; } = string.Empty;
    public int TotalComandas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioComanda { get; set; }
    public decimal EficienciaVentas { get; set; }
}

/// <summary>
/// 🍽️ Análisis por producto
/// </summary>
public class AnalisisProductoDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioVenta { get; set; }
    public decimal PorcentajeVentas { get; set; }
}

/// <summary>
/// 🕐 Distribución horaria de ventas
/// </summary>
public class DistribucionHorariaDto
{
    public int Hora { get; set; }
    public int TotalComandas { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal PromedioComanda { get; set; }
    public decimal PorcentajeDiario { get; set; }
}

/// <summary>
/// 🔄 Comparativo con período anterior
/// </summary>
public class ComparativoPeriodoDto
{
    public DateTime FechaAnterior { get; set; }
    public MetricasBasicasDto MetricasAnteriores { get; set; } = new();
    public decimal VariacionComandas { get; set; }
    public decimal VariacionVentas { get; set; }
}

/// <summary>
/// 📅 Tendencia diaria
/// </summary>
public class TendenciaDiariaDto
{
    public DateTime Fecha { get; set; }
    public int TotalComandas { get; set; }
    public decimal MontoTotal { get; set; }
} 