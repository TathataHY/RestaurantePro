using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para reportes comerciales
/// </summary>
public class ReporteComercialDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre del reporte es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporteComercial TipoReporte { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public DateTime FechaGeneracion { get; set; }
    
    public string UsuarioGenerador { get; set; } = string.Empty;
    
    public bool EstaCompletado { get; set; }
    
    public string? ArchivoUrl { get; set; }
    
    public Dictionary<string, object> Parametros { get; set; } = new();
    
    public Dictionary<string, object> Resultados { get; set; } = new();
}

/// <summary>
/// DTO para reportes de inventario
/// </summary>
public class ReporteInventarioDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El nombre del reporte es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporteInventario TipoReporte { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public DateTime FechaGeneracion { get; set; }
    
    public string UsuarioGenerador { get; set; } = string.Empty;
    
    public bool EstaCompletado { get; set; }
    
    public string? ArchivoUrl { get; set; }
    
    public Dictionary<string, object> Parametros { get; set; } = new();
    
    public Dictionary<string, object> Resultados { get; set; } = new();
}

/// <summary>
/// DTO para filtros de reportes comerciales
/// </summary>
public class FiltroReporteComercialDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public TipoReporteComercial? TipoReporte { get; set; }
    public string? Categoria { get; set; }
    public string? Producto { get; set; }
    public string? Cliente { get; set; }
    public string? Mesa { get; set; }
    public string? Usuario { get; set; }
    public bool? SoloCompletados { get; set; }
    public bool? IncluirDetalles { get; set; }
}

/// <summary>
/// DTO para filtros de reportes de inventario
/// </summary>
public class FiltroReporteInventarioDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public TipoReporteInventario? TipoReporte { get; set; }
    public string? Categoria { get; set; }
    public string? Ingrediente { get; set; }
    public string? Proveedor { get; set; }
    public bool? SoloActivos { get; set; }
    public bool? SoloStockBajo { get; set; }
    public bool? SoloVencidos { get; set; }
    public bool? IncluirDetalles { get; set; }
}

/// <summary>
/// DTO para estadísticas de reportes
/// </summary>
public class EstadisticasReportesDto
{
    public int TotalReportesGenerados { get; set; }
    public int ReportesComerciales { get; set; }
    public int ReportesInventario { get; set; }
    public int ReportesHoy { get; set; }
    public int ReportesMes { get; set; }
    public int ReportesCompletados { get; set; }
    public int ReportesPendientes { get; set; }
    public double TiempoPromedioGeneracion { get; set; }
}

/// <summary>
/// DTO para crear un reporte comercial
/// </summary>
public class CrearReporteComercialRequest
{
    [Required(ErrorMessage = "El nombre del reporte es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporteComercial TipoReporte { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public Dictionary<string, object> Parametros { get; set; } = new();
}

/// <summary>
/// DTO para crear un reporte de inventario
/// </summary>
public class CrearReporteInventarioRequest
{
    [Required(ErrorMessage = "El nombre del reporte es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El tipo de reporte es obligatorio")]
    public TipoReporteInventario TipoReporte { get; set; }
    
    public DateTime FechaInicio { get; set; }
    
    public DateTime FechaFin { get; set; }
    
    public Dictionary<string, object> Parametros { get; set; } = new();
}

