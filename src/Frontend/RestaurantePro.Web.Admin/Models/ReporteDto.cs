using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO base para reportes del restaurante
/// </summary>
public class ReporteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoReporte Tipo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public DateTime FechaGeneracion { get; set; }
    public string GeneradoPor { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
    public string? Parametros { get; set; } // JSON con parámetros específicos
}

/// <summary>
/// DTO para reporte de ventas por período
/// </summary>
public class ReporteVentasDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public int TotalFacturas { get; set; }
    public decimal PromedioVentaPorComanda { get; set; }
    public decimal PromedioVentaPorFactura { get; set; }
    public List<VentaPorDiaDto> VentasPorDia { get; set; } = new();
    public List<VentaPorHoraDto> VentasPorHora { get; set; } = new();
    public List<VentaPorMeseroDto> VentasPorMesero { get; set; } = new();
    public List<VentaPorMesaDto> VentasPorMesa { get; set; } = new();
}

/// <summary>
/// DTO para ventas por día
/// </summary>
public class VentaPorDiaDto
{
    public DateTime Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public int TotalFacturas { get; set; }
    public decimal PromedioVenta { get; set; }
}

/// <summary>
/// DTO para ventas por hora
/// </summary>
public class VentaPorHoraDto
{
    public int Hora { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public decimal PromedioVenta { get; set; }
}

/// <summary>
/// DTO para ventas por mesero
/// </summary>
public class VentaPorMeseroDto
{
    public Guid MeseroId { get; set; }
    public string MeseroNombre { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public decimal PromedioVenta { get; set; }
    public decimal Comision { get; set; }
}

/// <summary>
/// DTO para ventas por mesa
/// </summary>
public class VentaPorMesaDto
{
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int TotalComandas { get; set; }
    public decimal PromedioVenta { get; set; }
    public TimeSpan TiempoPromedioOcupacion { get; set; }
}

/// <summary>
/// DTO para reporte de productos más vendidos
/// </summary>
public class ReporteProductosDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public List<ProductoVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<ProductoVendidoDto> ProductosMenosVendidos { get; set; } = new();
    public List<ProductoVendidoDto> ProductosPorCategoria { get; set; } = new();
    public decimal TotalIngresos { get; set; }
    public int TotalProductosVendidos { get; set; }
}

/// <summary>
/// DTO para producto vendido
/// </summary>
public class ProductoVendidoDto
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string CategoriaNombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CantidadVendida { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
    public int VecesPedido { get; set; }
}

/// <summary>
/// DTO para reporte de rendimiento de mesas
/// </summary>
public class ReporteMesasDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalMesas { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
    public TimeSpan TiempoPromedioOcupacion { get; set; }
    public List<MesaRendimientoDto> RendimientoPorMesa { get; set; } = new();
    public List<MesaRendimientoDto> MesasMasRentables { get; set; } = new();
    public List<MesaRendimientoDto> MesasMenosRentables { get; set; } = new();
}

/// <summary>
/// DTO para rendimiento de mesa
/// </summary>
public class MesaRendimientoDto
{
    public Guid MesaId { get; set; }
    public string MesaNombre { get; set; } = string.Empty;
    public int NumeroMesa { get; set; }
    public int TotalComandas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioVenta { get; set; }
    public TimeSpan TiempoTotalOcupacion { get; set; }
    public TimeSpan TiempoPromedioOcupacion { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
    public int VecesOcupada { get; set; }
}

/// <summary>
/// DTO para reporte de resumen de comandas
/// </summary>
public class ReporteComandasDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalComandas { get; set; }
    public int ComandasPendientes { get; set; }
    public int ComandasEnProceso { get; set; }
    public int ComandasCompletadas { get; set; }
    public int ComandasCanceladas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioVentaPorComanda { get; set; }
    public TimeSpan TiempoPromedioPreparacion { get; set; }
    public List<ComandaRendimientoDto> ComandasPorEstado { get; set; } = new();
    public List<ComandaRendimientoDto> ComandasPorMesero { get; set; } = new();
    public List<ComandaRendimientoDto> ComandasPorHora { get; set; } = new();
}

/// <summary>
/// DTO para rendimiento de comanda
/// </summary>
public class ComandaRendimientoDto
{
    public Guid ComandaId { get; set; }
    public string NumeroComanda { get; set; } = string.Empty;
    public string MesaNombre { get; set; } = string.Empty;
    public string MeseroNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCompletada { get; set; }
    public TimeSpan? TiempoPreparacion { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalProductos { get; set; }
}

/// <summary>
/// DTO para filtros de reportes
/// </summary>
public class ReporteFiltrosDto
{
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime FechaFin { get; set; } = DateTime.Today;
    public TipoReporte? Tipo { get; set; }
    public Guid? MeseroId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? OrdenarPor { get; set; } = "Fecha";
    public string? DireccionOrden { get; set; } = "desc";
    public int? LimiteResultados { get; set; } = 100;
}

/// <summary>
/// DTO para solicitud de reporte
/// </summary>
public class SolicitarReporteRequest
{
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporte Tipo { get; set; }
    
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);
    
    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    public DateTime FechaFin { get; set; } = DateTime.Today;
    
    public Guid? MeseroId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public string? OrdenarPor { get; set; } = "Fecha";
    public string? DireccionOrden { get; set; } = "desc";
    public int? LimiteResultados { get; set; } = 100;
    public bool IncluirDetalles { get; set; } = true;
    public string? Formato { get; set; } = "JSON"; // JSON, PDF, Excel
}

/// <summary>
/// DTO para estadísticas generales de reportes
/// </summary>
public class ReporteEstadisticasDto
{
    public int TotalReportesGenerados { get; set; }
    public int ReportesHoy { get; set; }
    public int ReportesEstaSemana { get; set; }
    public int ReportesEsteMes { get; set; }
    public TipoReporte TipoMasSolicitado { get; set; }
    public DateTime UltimoReporteGenerado { get; set; }
    public List<ReporteDto> ReportesRecientes { get; set; } = new();
}

/// <summary>
/// Tipos de reportes disponibles
/// </summary>
public enum TipoReporte
{
    VentasPorPeriodo = 1,
    ProductosMasVendidos = 2,
    RendimientoMesas = 3,
    ResumenComandas = 4,
    VentasPorMesero = 5,
    VentasPorMesa = 6,
    VentasPorHora = 7,
    VentasPorDia = 8,
    ProductosPorCategoria = 9,
    ComandasPorEstado = 10
}

/// <summary>
/// DTO para exportación de reportes
/// </summary>
public class ExportarReporteRequest
{
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporte Tipo { get; set; }
    
    [Required(ErrorMessage = "El formato es obligatorio")]
    public string Formato { get; set; } = "PDF"; // PDF, Excel, CSV
    
    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    public DateTime FechaInicio { get; set; }
    
    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    public DateTime FechaFin { get; set; }
    
    public Guid? MeseroId { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? CategoriaId { get; set; }
    public bool IncluirGraficos { get; set; } = true;
    public bool IncluirDetalles { get; set; } = true;
}
