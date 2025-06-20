namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.VerificarDisponibilidad;

/// <summary>
/// Query para verificar disponibilidad de reservaciones
/// </summary>
public class VerificarDisponibilidadQuery : IRequest<Result<DisponibilidadDto>>
{
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public int NumeroPersonas { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ReservacionId { get; set; } // Para excluir la reservación actual en caso de modificación
} 