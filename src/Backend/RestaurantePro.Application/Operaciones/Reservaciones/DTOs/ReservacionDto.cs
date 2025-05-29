namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

public class ReservacionDto : BaseDto
{
    public Guid? ClienteId { get; set; }
    public string? NombreCliente { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string TelefonoContacto { get; set; } = string.Empty;
    public string? EmailContacto { get; set; }
    
    public DateTime FechaReservacion { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    public int CantidadPersonas { get; set; }
    
    public Guid? MesaId { get; set; }
    public int? NumeroMesa { get; set; }
    
    public EstadoReservacion Estado { get; set; }
    public string EstadoTexto => Estado.ToString();
    
    public string? Observaciones { get; set; }
    public string? ObservacionesCancelacion { get; set; }
    
    // Información de confirmación
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaCancelacion { get; set; }
    public DateTime? FechaLlegada { get; set; }
    
    // Estados calculados
    public bool EstaPendiente => Estado == EstadoReservacion.Pendiente;
    public bool EstaConfirmada => Estado == EstadoReservacion.Confirmada;
    public bool EstaCancelada => Estado == EstadoReservacion.Cancelada;
    public bool EstaCompletada => Estado == EstadoReservacion.Completada;
    
    public DateTime FechaHoraCompleta => FechaReservacion.Date + HoraReservacion;
    public bool EsParaHoy => FechaReservacion.Date == DateTime.Today;
    public bool EsPasada => FechaHoraCompleta < DateTime.Now;
} 