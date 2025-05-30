using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

/// <summary>
/// Command para liberar una mesa (marcarla como disponible)
/// </summary>
public class LiberarMesaCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// ID de la mesa a liberar
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// ID del mesero que libera la mesa (opcional)
    /// </summary>
    public Guid? MeseroId { get; set; }

    /// <summary>
    /// Observaciones sobre la liberación (opcional)
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Factory method para crear el command con parámetros mínimos
    /// </summary>
    public static LiberarMesaCommand Crear(Guid mesaId, Guid? meseroId = null, string? observaciones = null)
    {
        return new LiberarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            Observaciones = observaciones
        };
    }
} 