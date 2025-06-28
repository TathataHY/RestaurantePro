using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAlertasInventario;

/// <summary>
/// Query para obtener alertas de inventario
/// </summary>
public class ObtenerAlertasInventarioQuery : IRequest<Result<List<AlertaInventarioDto>>>
{
    /// <summary>
    /// Si incluir solo alertas críticas
    /// </summary>
    public bool SoloCriticas { get; set; } = false;
    
    /// <summary>
    /// Categoría específica para filtrar alertas
    /// </summary>
    public string? Categoria { get; set; }
} 