using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorId;

/// <summary>
/// Query para obtener una mesa específica por ID
/// </summary>
public class ObtenerMesaPorIdQuery : IRequest<Result<MesaDto>>
{
    /// <summary>
    /// ID de la mesa a obtener
    /// </summary>
    public Guid Id { get; set; }
} 