using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerColaPreparaciones;

/// <summary>
/// Query para obtener la cola de preparaciones pendientes
/// </summary>
public class ObtenerColaPreparacionesQuery : IRequest<Result<List<PreparacionDto>>>
{
    public EstadoPreparacion? Estado { get; set; }
    public Guid? ChefId { get; set; }
    public bool OrdenarPorPrioridad { get; set; } = true;
    public int? Limite { get; set; }
} 