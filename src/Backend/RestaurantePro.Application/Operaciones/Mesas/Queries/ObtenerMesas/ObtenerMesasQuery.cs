using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;

/// <summary>
/// Query para obtener mesas con filtros opcionales y paginación
/// </summary>
public class ObtenerMesasQuery : IRequest<Result<PaginatedList<MesaDto>>>
{
    /// <summary>
    /// Filtro opcional por estado de mesa
    /// </summary>
    public string? Estado { get; set; }
    
    /// <summary>
    /// Filtro opcional por ubicación
    /// </summary>
    public string? Ubicacion { get; set; }
    
    /// <summary>
    /// Filtro opcional por capacidad mínima
    /// </summary>
    public int? CapacidadMinima { get; set; }
    
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; } = 10;
} 