namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ActualizarReservacion;

/// <summary>
/// Command para actualizar una reservación existente
/// </summary>
public class ActualizarReservacionCommand : IRequest<Result<ReservacionDto>>
{
    public Guid Id { get; set; }
    public DateTime FechaReservacion { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    public int NumeroPersonas { get; set; }
    public string? Observaciones { get; set; }
    public Guid? MesaId { get; set; }
    public Guid? UsuarioId { get; set; }
} 