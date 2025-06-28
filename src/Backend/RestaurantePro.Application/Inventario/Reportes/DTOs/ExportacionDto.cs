namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

public class FiltrosExportacionDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public List<string>? Categorias { get; set; }
    public List<int>? IngredienteIds { get; set; }
    public string? Formato { get; set; } = "Excel";
    public bool IncluirDetalles { get; set; } = true;
}

public class ReporteExportadoDto
{
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string Formato { get; set; } = string.Empty;
    public long TamañoBytes { get; set; }
    public DateTime FechaExportacion { get; set; }
    public string UrlDescarga { get; set; } = string.Empty;
} 