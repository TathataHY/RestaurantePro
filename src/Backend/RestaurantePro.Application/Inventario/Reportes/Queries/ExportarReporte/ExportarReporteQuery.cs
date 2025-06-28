using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ExportarReporte;

/// <summary>
/// Query para exportar reportes de inventario
/// </summary>
public class ExportarReporteQuery : IRequest<Result<ReporteExportadoDto>>
{
    /// <summary>
    /// Tipo de reporte a exportar
    /// </summary>
    public string TipoReporte { get; set; } = string.Empty;
    
    /// <summary>
    /// Formato de exportación
    /// </summary>
    public string Formato { get; set; } = "Excel";
} 