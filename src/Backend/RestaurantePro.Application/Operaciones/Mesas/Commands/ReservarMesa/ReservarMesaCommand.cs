using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.ReservarMesa;

/// <summary>
/// Command para reservar una mesa
/// </summary>
public class ReservarMesaCommand : IRequest<Result<Unit>>
{
    public Guid MesaId { get; set; }
    public Guid ClienteId { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public string? Observaciones { get; set; }
    public int DuracionMinutos { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
} 