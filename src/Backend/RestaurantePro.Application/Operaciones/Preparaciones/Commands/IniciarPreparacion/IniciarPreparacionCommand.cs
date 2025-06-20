using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.IniciarPreparacion;

/// <summary>
/// Command para iniciar una preparación
/// </summary>
public class IniciarPreparacionCommand : IRequest<Result<PreparacionDto>>
{
    public Guid Id { get; set; }
    public Guid ChefId { get; set; }
    public string? Observaciones { get; set; }
} 