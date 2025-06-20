namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ReprogramarReservacion;

/// <summary>
/// Command para reprogramar una reservación
/// </summary>
public class ReprogramarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    public Guid Id { get; set; }
    public DateTime NuevaFechaReservacion { get; set; }
    public TimeSpan NuevaHoraReservacion { get; set; }
    public int? NuevoNumeroPersonas { get; set; }
    public string? MotivoReprogramacion { get; set; }
    public Guid? UsuarioId { get; set; }
} 