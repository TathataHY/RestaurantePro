using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// DTO para estadísticas de inventario
/// </summary>
public class InventarioEstadisticasDto
{
    /// <summary>
    /// Total de ingredientes
    /// </summary>
    public int TotalIngredientes { get; set; }

    /// <summary>
    /// Ingredientes activos
    /// </summary>
    public int IngredientesActivos { get; set; }

    /// <summary>
    /// Ingredientes inactivos
    /// </summary>
    public int IngredientesInactivos { get; set; }

    /// <summary>
    /// Ingredientes con stock bajo
    /// </summary>
    public int IngredientesStockBajo { get; set; }

    /// <summary>
    /// Ingredientes sin stock
    /// </summary>
    public int IngredientesSinStock { get; set; }

    /// <summary>
    /// Ingredientes próximos a vencer
    /// </summary>
    public int IngredientesProximoVencer { get; set; }

    /// <summary>
    /// Ingredientes vencidos
    /// </summary>
    public int IngredientesVencidos { get; set; }

    /// <summary>
    /// Valor total del inventario
    /// </summary>
    public decimal ValorTotalInventario { get; set; }

    /// <summary>
    /// Valor promedio por ingrediente
    /// </summary>
    public decimal ValorPromedioIngrediente { get; set; }

    /// <summary>
    /// Ingrediente más valioso
    /// </summary>
    public string? IngredienteMasValioso { get; set; }

    /// <summary>
    /// Valor del ingrediente más valioso
    /// </summary>
    public decimal ValorIngredienteMasValioso { get; set; }

    /// <summary>
    /// Ingrediente con mayor rotación
    /// </summary>
    public string? IngredienteMayorRotacion { get; set; }

    /// <summary>
    /// Ingrediente con menor rotación
    /// </summary>
    public string? IngredienteMenorRotacion { get; set; }

    /// <summary>
    /// Total de movimientos en el período
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Movimientos de entrada
    /// </summary>
    public int MovimientosEntrada { get; set; }

    /// <summary>
    /// Movimientos de salida
    /// </summary>
    public int MovimientosSalida { get; set; }

    /// <summary>
    /// Fecha de la última actualización
    /// </summary>
    public DateTime FechaUltimaActualizacion { get; set; }

    /// <summary>
    /// Ingredientes con stock bajo
    /// </summary>
    public int StockBajo { get; set; }

    /// <summary>
    /// Ingredientes próximos a vencer
    /// </summary>
    public int VencimientoProximo { get; set; }

    /// <summary>
    /// Movimientos de hoy
    /// </summary>
    public int MovimientosHoy { get; set; }

    /// <summary>
    /// Ingredientes vencidos
    /// </summary>
    public int Vencidos { get; set; }

    /// <summary>
    /// Valor del stock bajo
    /// </summary>
    public decimal ValorStockBajo { get; set; }

    /// <summary>
    /// Valor de ingredientes vencidos
    /// </summary>
    public decimal ValorVencidos { get; set; }

    /// <summary>
    /// Movimientos del mes
    /// </summary>
    public int MovimientosMes { get; set; }

    /// <summary>
    /// Costo total de movimientos de hoy
    /// </summary>
    public decimal CostoTotalMovimientosHoy { get; set; }

    /// <summary>
    /// Costo total de movimientos del mes
    /// </summary>
    public decimal CostoTotalMovimientosMes { get; set; }
}
