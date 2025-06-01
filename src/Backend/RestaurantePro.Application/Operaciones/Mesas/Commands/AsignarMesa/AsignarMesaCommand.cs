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
    /// ID de la reservación asociada (si aplica)
    /// </summary>
    public Guid? ReservacionId { get; set; }

    /// <summary>
    /// Tipo de asignación de mesa
    /// </summary>
    public string TipoAsignacion { get; set; } = "Manual";

    /// <summary>
    /// Indica si se debe validar la reservación
    /// </summary>
    public bool ValidarReservacion { get; set; } = true;

    /// <summary>
    /// Número de personas para la mesa
    /// </summary>
    public int? NumeroPersonas { get; set; }

    /// <summary>
    /// Indica si debe buscar la mejor mesa disponible
    /// </summary>
    public bool BuscarMejorMesa { get; set; } = false;

    /// <summary>
    /// Indica si debe notificar al mesero
    /// </summary>
    public bool NotificarMesero { get; set; } = true;

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