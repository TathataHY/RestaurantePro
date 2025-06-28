using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerReporteGeneral;

/// <summary>
/// Query para obtener reporte general de inventario
/// </summary>
public class ObtenerReporteGeneralQuery : IRequest<Result<ReporteGeneralInventarioDto>>
{
    /// <summary>
    /// Si incluir detalles de ingredientes
    /// </summary>
    public bool IncluirDetalles { get; set; } = true;
    
    /// <summary>
    /// Categoría específica para filtrar
    /// </summary>
    public string? Categoria { get; set; }
} 