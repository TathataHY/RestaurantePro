namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;

/// <summary>
/// Command para asignar (ocupar) una mesa en el restaurante
/// </summary>
public class AsignarMesaCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// ID de la mesa a asignar
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// ID del mesero que asigna la mesa (opcional)
    /// </summary>
    public Guid? MeseroId { get; set; }

    /// <summary>
    /// Observaciones sobre la asignación (opcional)
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Factory method para crear el command con parámetros mínimos
    /// </summary>
    public static AsignarMesaCommand Crear(Guid mesaId, Guid? meseroId = null, string? observaciones = null)
    {
        return new AsignarMesaCommand
        {
            MesaId = mesaId,
            MeseroId = meseroId,
            Observaciones = observaciones
        };
    }
} 