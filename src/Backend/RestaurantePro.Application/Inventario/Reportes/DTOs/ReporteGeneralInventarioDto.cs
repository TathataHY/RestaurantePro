namespace RestaurantePro.Application.Inventario.Reportes.DTOs;

/// <summary>
/// DTO para el reporte general de inventario
/// </summary>
public class ReporteGeneralInventarioDto
{
    /// <summary>
    /// Fecha de generación del reporte
    /// </summary>
    public DateTime FechaGeneracion { get; set; }
    
    /// <summary>
    /// Total de ingredientes en inventario
    /// </summary>
    public int TotalIngredientes { get; set; }
    
    /// <summary>
    /// Valor total del inventario
    /// </summary>
    public decimal ValorTotalInventario { get; set; }
    
    /// <summary>
    /// Cantidad de ingredientes con stock bajo
    /// </summary>
    public int IngredientesStockBajo { get; set; }
    
    /// <summary>
    /// Cantidad de ingredientes con stock crítico
    /// </summary>
    public int IngredientesStockCritico { get; set; }
    
    /// <summary>
    /// Lista de ingredientes incluidos en el reporte
    /// </summary>
    public List<IngredienteReporteDto> Ingredientes { get; set; } = new();
    
    /// <summary>
    /// Resumen por categorías
    /// </summary>
    public List<CategoriaResumenDto> ResumenPorCategorias { get; set; } = new();
}

/// <summary>
/// DTO para ingrediente en el reporte
/// </summary>
public class IngredienteReporteDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string EstadoStock { get; set; } = string.Empty; // "Normal", "Bajo", "Crítico"
    public DateTime? UltimaActualizacion { get; set; }
}

/// <summary>
/// DTO para resumen por categorías
/// </summary>
public class CategoriaResumenDto
{
    public string Categoria { get; set; } = string.Empty;
    public int CantidadIngredientes { get; set; }
    public decimal ValorTotal { get; set; }
    public int StockBajo { get; set; }
    public int StockCritico { get; set; }
} 