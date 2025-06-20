using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacion;

/// <summary>
/// Command para actualizar una preparación existente
/// </summary>
public class ActualizarPreparacionCommand : IRequest<Result<PreparacionDto>>
{
    public Guid Id { get; set; }
    public int? Cantidad { get; set; }
    public int? Prioridad { get; set; }
    public TimeSpan? TiempoEstimado { get; set; }
    public string? Observaciones { get; set; }
    public Guid? ChefId { get; set; }
} 