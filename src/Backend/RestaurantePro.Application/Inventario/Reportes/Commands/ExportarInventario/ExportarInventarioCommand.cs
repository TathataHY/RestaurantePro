using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Commands.ExportarInventario;

/// <summary>
/// Comando para exportar reporte de inventario
/// </summary>
public class ExportarInventarioCommand : IRequest<Result<ReporteExportadoDto>>
{
    /// <summary>
    /// Formato de exportación (PDF, Excel, CSV)
    /// </summary>
    public string Formato { get; set; } = string.Empty;
    
    /// <summary>
    /// Si incluir movimientos de inventario
    /// </summary>
    public bool IncluirMovimientos { get; set; }
    
    /// <summary>
    /// Fecha desde para filtrar datos
    /// </summary>
    public DateTime? FechaDesde { get; set; }
    
    /// <summary>
    /// Fecha hasta para filtrar datos
    /// </summary>
    public DateTime? FechaHasta { get; set; }
}

/// <summary>
/// DTO para filtros de exportación
/// </summary>
public class FiltrosExportacionDto
{
    /// <summary>
    /// Solo ingredientes con stock bajo
    /// </summary>
    public bool SoloStockBajo { get; set; }

    /// <summary>
    /// Solo ingredientes activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Incluir solo ingredientes críticos
    /// </summary>
    public bool SoloCriticos { get; set; }
} 