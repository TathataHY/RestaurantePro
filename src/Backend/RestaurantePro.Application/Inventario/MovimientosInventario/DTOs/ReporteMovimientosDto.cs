namespace RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;

/// <summary>
/// DTO para reporte de movimientos de inventario
/// </summary>
public class ReporteMovimientosDto
{
    /// <summary>
    /// Período del reporte
    /// </summary>
    public string PeriodoReporte { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de generación del reporte
    /// </summary>
    public DateTime FechaGeneracion { get; set; }

    /// <summary>
    /// Total de movimientos en el período
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Total de ingresos en el período
    /// </summary>
    public decimal TotalIngresos { get; set; }

    /// <summary>
    /// Total de egresos en el período
    /// </summary>
    public decimal TotalEgresos { get; set; }

    /// <summary>
    /// Balance neto (ingresos - egresos)
    /// </summary>
    public decimal BalanceNeto { get; set; }

    /// <summary>
    /// Valor total de los movimientos
    /// </summary>
    public decimal ValorTotalMovimientos { get; set; }

    /// <summary>
    /// Lista de movimientos del período
    /// </summary>
    public List<MovimientoInventarioDto> Movimientos { get; set; } = new();

    /// <summary>
    /// Resumen por tipo de movimiento
    /// </summary>
    public List<ResumenPorTipoDto>? ResumenPorTipo { get; set; }

    /// <summary>
    /// Resumen por ingrediente
    /// </summary>
    public List<ResumenPorIngredienteDto>? ResumenPorIngrediente { get; set; }

    /// <summary>
    /// Análisis de tendencias
    /// </summary>
    public AnalisisTendenciasDto? AnalisisTendencias { get; set; }
}

/// <summary>
/// DTO para resumen por tipo de movimiento
/// </summary>
public class ResumenPorTipoDto
{
    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public string TipoMovimiento { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de movimientos
    /// </summary>
    public int CantidadMovimientos { get; set; }

    /// <summary>
    /// Cantidad total
    /// </summary>
    public decimal CantidadTotal { get; set; }

    /// <summary>
    /// Valor total
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Porcentaje del total
    /// </summary>
    public decimal PorcentajeDelTotal { get; set; }
}

/// <summary>
/// DTO para resumen por ingrediente
/// </summary>
public class ResumenPorIngredienteDto
{
    /// <summary>
    /// ID del ingrediente
    /// </summary>
    public Guid IngredienteId { get; set; }

    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string NombreIngrediente { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de movimientos
    /// </summary>
    public int CantidadMovimientos { get; set; }

    /// <summary>
    /// Cantidad total
    /// </summary>
    public decimal CantidadTotal { get; set; }

    /// <summary>
    /// Valor total
    /// </summary>
    public decimal ValorTotal { get; set; }

    /// <summary>
    /// Stock actual
    /// </summary>
    public decimal StockActual { get; set; }
}

/// <summary>
/// DTO para análisis de tendencias
/// </summary>
public class AnalisisTendenciasDto
{
    /// <summary>
    /// Variación porcentual respecto al período anterior
    /// </summary>
    public decimal VariacionPorcentual { get; set; }

    /// <summary>
    /// Tendencia general
    /// </summary>
    public string TendenciaGeneral { get; set; } = string.Empty;

    /// <summary>
    /// Tendencia diaria
    /// </summary>
    public List<TendenciaDiariaDto> TendenciaDiaria { get; set; } = new();

    /// <summary>
    /// Recomendaciones basadas en el análisis
    /// </summary>
    public List<string> Recomendaciones { get; set; } = new();
}

/// <summary>
/// DTO para tendencia diaria
/// </summary>
public class TendenciaDiariaDto
{
    /// <summary>
    /// Fecha
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Cantidad de movimientos
    /// </summary>
    public int CantidadMovimientos { get; set; }

    /// <summary>
    /// Cantidad total
    /// </summary>
    public decimal CantidadTotal { get; set; }

    /// <summary>
    /// Valor total
    /// </summary>
    public decimal ValorTotal { get; set; }
} 