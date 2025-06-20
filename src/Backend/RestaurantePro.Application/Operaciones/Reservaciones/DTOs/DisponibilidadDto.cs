using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO para representar la disponibilidad de reservaciones
/// </summary>
public class DisponibilidadDto
{
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public int NumeroPersonas { get; set; }
    public bool Disponible { get; set; }
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();
    public List<ReservacionExistenteDto> ReservacionesExistentes { get; set; } = new();
    public string? Mensaje { get; set; }
    public int CapacidadTotal { get; set; }
    public int CapacidadOcupada { get; set; }
    public int CapacidadDisponible { get; set; }
    public List<AlternativaDto> Alternativas { get; set; } = new();
    public EstadisticasOcupacionDto? EstadisticasOcupacion { get; set; }
    
    // Propiedades adicionales que el handler espera
    public bool HayDisponibilidad { get; set; }
    public DateTime FechaHoraConsultada { get; set; }
    public int NumeroPersonasSolicitadas { get; set; }
    public string? MotivoNoDisponibilidad { get; set; }
    public List<AlternativaDto> AlternativasSugeridas { get; set; } = new();
}

/// <summary>
/// DTO para representar una mesa disponible
/// </summary>
public class MesaDisponibleDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public bool EsCombinable { get; set; }
    public string? Estado { get; set; }
    
    // Propiedades adicionales que el handler espera
    public Guid MesaId { get; set; }
    public string Zona { get; set; } = string.Empty;
    public bool Disponible { get; set; }
    public List<string> Caracteristicas { get; set; } = new();
    public decimal PrecioBase { get; set; }
    public bool EsVIP { get; set; }
    public bool TieneVentana { get; set; }
    public DateTime? ProximaDisponibilidad { get; set; }
}

/// <summary>
/// DTO para representar una reservación existente
/// </summary>
public class ReservacionExistenteDto
{
    public Guid Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public int NumeroPersonas { get; set; }
    public TimeSpan HoraReservacion { get; set; }
    public EstadoReservacion Estado { get; set; }
    public string? NumeroMesa { get; set; }
}

/// <summary>
/// DTO para representar alternativas de reservación
/// </summary>
public class AlternativaDto
{
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public int NumeroPersonas { get; set; }
    public bool Disponible { get; set; }
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();
    public string? Mensaje { get; set; }
    
    // Propiedades adicionales que el handler espera
    public DateTime FechaHora { get; set; }
    public int CantidadMesasDisponibles { get; set; }
    public bool MejorOpcion { get; set; }
    public int DiferenciaMinutos { get; set; }
}

/// <summary>
/// DTO para estadísticas de ocupación
/// </summary>
public class EstadisticasOcupacionDto
{
    public DateTime Fecha { get; set; }
    public int TotalMesas { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public double PorcentajeOcupacion { get; set; }
    public List<OcupacionPorHoraDto> OcupacionPorHora { get; set; } = new();
    
    // Propiedades adicionales que el handler espera
    public double NivelOcupacion { get; set; }
}

/// <summary>
/// DTO para ocupación por hora
/// </summary>
public class OcupacionPorHoraDto
{
    public TimeSpan Hora { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasDisponibles { get; set; }
    public double PorcentajeOcupacion { get; set; }
} 