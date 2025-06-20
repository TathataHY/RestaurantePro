using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Commands.CancelarPreparacion;

/// <summary>
/// Command para cancelar una preparación
/// </summary>
public class CancelarPreparacionCommand : IRequest<Result<PreparacionDto>>
{
    public Guid Id { get; set; }
    public string MotivoCancelacion { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
} 