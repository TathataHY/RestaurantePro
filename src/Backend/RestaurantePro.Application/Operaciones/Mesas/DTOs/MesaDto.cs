namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

public class MesaDto : BaseDto
{
    public int Numero { get; set; }
    // Comentado temporalmente hasta encontrar el enum correcto
    // public ZonaMesa Zona { get; set; }
    public string Zona { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public bool Disponible { get; set; }
    public EstadoMesa Estado { get; set; }
    public string EstadoTexto => Estado.ToString();
    public string? Descripcion { get; set; }
    public bool Activa { get; set; }
    
    /// <summary>
    /// Ubicación específica de la mesa (compatibilidad con tests)
    /// </summary>
    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la mesa (compatibilidad con tests)
    /// </summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de mesa
    /// </summary>
    public TipoMesa Tipo { get; set; } = TipoMesa.Estandar;

    /// <summary>
    /// ID del mesero asignado (compatibilidad con tests)
    /// </summary>
    public Guid? MeseroAsignadoId { get; set; }

    /// <summary>
    /// Fecha de asignación del mesero (compatibilidad con tests)
    /// </summary>
    public DateTime? FechaAsignacion { get; set; }
    
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