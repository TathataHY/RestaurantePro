using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerPreparacionPorId;

/// <summary>
/// Query para obtener una preparación específica por ID
/// </summary>
public class ObtenerPreparacionPorIdQuery : IRequest<Result<PreparacionDto>>
{
    public Guid Id { get; set; }
} 