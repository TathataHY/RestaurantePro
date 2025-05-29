namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

/// <summary>
/// Command para confirmar una reservación pendiente
/// Permite confirmar reservaciones que requieren validación previa
/// </summary>
public class ConfirmarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    /// <summary>
    /// ID de la reservación a confirmar
    /// </summary>
    public Guid ReservacionId { get; set; }

    /// <summary>
    /// Código de reservación (alternativo al ID)
    /// </summary>
    public string? CodigoReservacion { get; set; }

    /// <summary>
    /// Método de confirmación utilizado
    /// </summary>
    public string MetodoConfirmacion { get; set; } = "Manual";

    /// <summary>
    /// Usuario o persona que confirma la reservación
    /// </summary>
    public string? ConfirmadoPor { get; set; }

    /// <summary>
    /// Notas adicionales de la confirmación
    /// </summary>
    public string? NotasConfirmacion { get; set; }

    /// <summary>
    /// Indica si se debe notificar al cliente
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Datos adicionales de la confirmación
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    public ConfirmarReservacionCommand(Guid reservacionId)
    {
        ReservacionId = reservacionId;
    }

    public ConfirmarReservacionCommand(string codigoReservacion)
    {
        CodigoReservacion = codigoReservacion;
    }

    public ConfirmarReservacionCommand() { }
} 