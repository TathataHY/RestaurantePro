namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

public class MesaDto : BaseDto
{
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public ZonaMesa Zona { get; set; }
    public string ZonaTexto => Zona.ToString();
    public EstadoMesa Estado { get; set; }
    public string EstadoTexto => Estado.ToString();
    public string? Descripcion { get; set; }
    public bool Activa { get; set; }
    
    // Información de ocupación actual
    public Guid? ComandaActualId { get; set; }
    public int? NumeroComandaActual { get; set; }
    public DateTime? FechaOcupacion { get; set; }
    public TimeSpan? TiempoOcupada => FechaOcupacion.HasValue && Estado == EstadoMesa.Ocupada 
        ? DateTime.Now - FechaOcupacion.Value 
        : null;
    
    // Estados calculados
    public bool EstaDisponible => Estado == EstadoMesa.Disponible && Activa;
    public bool EstaOcupada => Estado == EstadoMesa.Ocupada;
    public bool EstaReservada => Estado == EstadoMesa.Reservada;
} 