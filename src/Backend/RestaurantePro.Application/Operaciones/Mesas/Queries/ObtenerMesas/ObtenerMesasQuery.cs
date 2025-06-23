using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas;

/// <summary>
/// Query para obtener todas las mesas con filtros opcionales
/// </summary>
public class ObtenerMesasQuery : IRequest<Result<List<MesaDto>>>
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
} 