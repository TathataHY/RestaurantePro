using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ActualizarMesa;

/// <summary>
/// Command para actualizar una mesa existente
/// </summary>
public class ActualizarMesaCommand : IRequest<Result<MesaDto>>
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
} 