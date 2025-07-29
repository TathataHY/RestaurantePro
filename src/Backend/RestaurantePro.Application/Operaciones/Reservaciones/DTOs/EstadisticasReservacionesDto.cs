namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO para estadísticas de reservaciones
/// </summary>
public class EstadisticasReservacionesDto
{
    public int TotalReservaciones { get; set; }
    public int ReservacionesConfirmadas { get; set; }
    public int ReservacionesPendientes { get; set; }
    public int ReservacionesCanceladas { get; set; }
    public int ReservacionesCompletadas { get; set; }
    public decimal TasaOcupacionPromedio { get; set; }
    public List<ReservacionDto> ReservacionesHoy { get; set; } = new();
    public List<ReservacionDto> ReservacionesProximas { get; set; } = new();
} 