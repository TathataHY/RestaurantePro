using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CompletarPreparacion;

/// <summary>
/// Command para completar una preparación
/// </summary>
public class CompletarPreparacionCommand : IRequest<Result<PreparacionDto>>
{
    public Guid Id { get; set; }
    public TimeSpan? TiempoReal { get; set; }
    public string? Observaciones { get; set; }
} 