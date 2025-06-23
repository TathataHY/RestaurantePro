using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarCliente;

/// <summary>
/// Command para asignar un cliente a una mesa
/// </summary>
public class AsignarClienteAMesaCommand : IRequest<Result<Unit>>
{
    public Guid MesaId { get; set; }
    public Guid ClienteId { get; set; }
    public string? Observaciones { get; set; }
} 