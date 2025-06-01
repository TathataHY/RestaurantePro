namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;

/// <summary>
/// Command para modificar una reservación existente
/// </summary>
public class ModificarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    /// <summary>
    /// ID de la reservación a modificar
    /// </summary>
    public Guid ReservacionId { get; set; }

    /// <summary>
    /// Nueva fecha de la reservación
    /// </summary>
    public DateTime NuevaFechaReservacion { get; set; }

    /// <summary>
    /// Nueva hora de la reservación
    /// </summary>
    public TimeSpan NuevaHoraReservacion { get; set; }

    /// <summary>
    /// Nuevo número de personas
    /// </summary>
    public int NuevoNumeroPersonas { get; set; }

    /// <summary>
    /// Nuevo ID de mesa (opcional)
    /// </summary>
    public Guid? NuevaMesaId { get; set; }

    /// <summary>
    /// Nuevo ID de cliente (opcional)
    /// </summary>
    public Guid? NuevoClienteId { get; set; }

    /// <summary>
    /// Motivo de la modificación
    /// </summary>
    public string MotivoModificacion { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones adicionales sobre la modificación
    /// </summary>
    public string? ObservacionesModificacion { get; set; }

    /// <summary>
    /// ID del usuario que realiza la modificación
    /// </summary>
    public Guid UsuarioId { get; set; }
} 