using MediatR;
using RestaurantePro.Application.Common.Behaviors;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerValorTotalInventario;

/// <summary>
/// Query para obtener valor total del inventario
/// </summary>
public class ObtenerValorTotalInventarioQuery : IRequest<Result<ValorTotalInventarioDto>>
{
    /// <summary>
    /// Categoría específica para filtrar
    /// </summary>
    public string? Categoria { get; set; }
    
    /// <summary>
    /// Si incluir solo ingredientes activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Incluir solo ingredientes con stock
    /// </summary>
    public bool SoloConStock { get; set; } = false;
} 