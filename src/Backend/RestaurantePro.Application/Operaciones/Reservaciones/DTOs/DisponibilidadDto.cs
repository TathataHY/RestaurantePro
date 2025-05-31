namespace RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

/// <summary>
/// DTO para representar la disponibilidad de mesas para una consulta
/// </summary>
public class DisponibilidadDto
{
    /// <summary>
    /// Fecha y hora consultada
    /// </summary>
    public DateTime FechaHoraConsultada { get; set; }

    /// <summary>
    /// Número de personas solicitadas
    /// </summary>
    public int NumeroPersonasSolicitadas { get; set; }

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    public int DuracionEstimadaMinutos { get; set; }

    /// <summary>
    /// Zona específica consultada (si aplica)
    /// </summary>
    public string? ZonaConsultada { get; set; }

    /// <summary>
    /// Indica si hay disponibilidad para la consulta realizada
    /// </summary>
    public bool HayDisponibilidad { get; set; }

    /// <summary>
    /// Lista de mesas disponibles
    /// </summary>
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();

    /// <summary>
    /// Alternativas sugeridas si no hay disponibilidad exacta
    /// </summary>
    public List<AlternativaDto> AlternativasSugeridas { get; set; } = new();

    /// <summary>
    /// Estadísticas de ocupación general
    /// </summary>
    public EstadisticasOcupacionDto? EstadisticasOcupacion { get; set; }

    /// <summary>
    /// Capacidad total disponible en el horario consultado
    /// </summary>
    public int CapacidadTotalDisponible { get; set; }

    /// <summary>
    /// Número total de mesas disponibles
    /// </summary>
    public int TotalMesasDisponibles => MesasDisponibles.Count;

    /// <summary>
    /// Mesa recomendada (mejor opción según criterios)
    /// </summary>
    public MesaDisponibleDto? MesaRecomendada => MesasDisponibles.FirstOrDefault();

    /// <summary>
    /// Tiempo de respuesta de la consulta en milisegundos
    /// </summary>
    public long TiempoRespuestaMs { get; set; }

    /// <summary>
    /// Observaciones adicionales sobre la disponibilidad
    /// </summary>
    public string? Observaciones { get; set; }
}

/// <summary>
/// DTO para representar una mesa disponible
/// </summary>
public class MesaDisponibleDto
{
    /// <summary>
    /// ID de la mesa
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la mesa
    /// </summary>
    public int Numero { get; set; }

    /// <summary>
    /// Zona donde se encuentra la mesa
    /// </summary>
    public string Zona { get; set; } = string.Empty;

    /// <summary>
    /// Capacidad de la mesa
    /// </summary>
    public int Capacidad { get; set; }

    /// <summary>
    /// Descripción de la mesa
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Precio base por hora (si aplica)
    /// </summary>
    public decimal PrecioBase { get; set; }

    /// <summary>
    /// Características especiales de la mesa
    /// </summary>
    public List<string> Caracteristicas { get; set; } = new();

    /// <summary>
    /// Disponibilidad exacta para el horario consultado
    /// </summary>
    public bool DisponibleEnHorarioExacto { get; set; }

    /// <summary>
    /// Horario desde el cual está disponible
    /// </summary>
    public DateTime DisponibleDesde { get; set; }

    /// <summary>
    /// Horario hasta el cual está disponible
    /// </summary>
    public DateTime DisponibleHasta { get; set; }

    /// <summary>
    /// Reservación siguiente (si existe)
    /// </summary>
    public ReservacionSiguienteDto? ReservacionSiguiente { get; set; }

    /// <summary>
    /// Ubicación específica (ventana, terraza, etc.)
    /// </summary>
    public string? Ubicacion { get; set; }

    /// <summary>
    /// Indica si es mesa premium
    /// </summary>
    public bool EsPremium { get; set; }

    /// <summary>
    /// Servicios adicionales disponibles en la mesa
    /// </summary>
    public List<string> ServiciosAdicionales { get; set; } = new();
}

/// <summary>
/// DTO para alternativas cuando no hay disponibilidad exacta
/// </summary>
public class AlternativaDto
{
    /// <summary>
    /// Tipo de alternativa (Horario, Capacidad, Zona)
    /// </summary>
    public string TipoAlternativa { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora alternativa sugerida
    /// </summary>
    public DateTime FechaHoraAlternativa { get; set; }

    /// <summary>
    /// Mesas disponibles en esta alternativa
    /// </summary>
    public List<MesaDisponibleDto> MesasDisponibles { get; set; } = new();

    /// <summary>
    /// Diferencia en minutos respecto al horario original
    /// </summary>
    public int DiferenciaMinutos { get; set; }

    /// <summary>
    /// Capacidad alternativa sugerida
    /// </summary>
    public int? CapacidadAlternativa { get; set; }

    /// <summary>
    /// Zona alternativa sugerida
    /// </summary>
    public string? ZonaAlternativa { get; set; }

    /// <summary>
    /// Nivel de recomendación (1-5, siendo 5 la mejor)
    /// </summary>
    public int NivelRecomendacion { get; set; }

    /// <summary>
    /// Explicación de por qué es una buena alternativa
    /// </summary>
    public string Justificacion { get; set; } = string.Empty;

    /// <summary>
    /// Precio diferencial respecto a la opción original
    /// </summary>
    public decimal? DiferenciaPrecio { get; set; }
}

/// <summary>
/// DTO para estadísticas de ocupación
/// </summary>
public class EstadisticasOcupacionDto
{
    /// <summary>
    /// Porcentaje de ocupación actual
    /// </summary>
    public decimal PorcentajeOcupacionActual { get; set; }

    /// <summary>
    /// Porcentaje de ocupación promedio para esta hora
    /// </summary>
    public decimal PorcentajeOcupacionPromedio { get; set; }

    /// <summary>
    /// Total de mesas activas
    /// </summary>
    public int TotalMesasActivas { get; set; }

    /// <summary>
    /// Total de mesas ocupadas
    /// </summary>
    public int TotalMesasOcupadas { get; set; }

    /// <summary>
    /// Total de mesas reservadas
    /// </summary>
    public int TotalMesasReservadas { get; set; }

    /// <summary>
    /// Total de mesas disponibles
    /// </summary>
    public int TotalMesasDisponibles { get; set; }

    /// <summary>
    /// Capacidad total del restaurante
    /// </summary>
    public int CapacidadTotal { get; set; }

    /// <summary>
    /// Capacidad ocupada actualmente
    /// </summary>
    public int CapacidadOcupada { get; set; }

    /// <summary>
    /// Capacidad reservada
    /// </summary>
    public int CapacidadReservada { get; set; }

    /// <summary>
    /// Capacidad disponible
    /// </summary>
    public int CapacidadDisponible { get; set; }

    /// <summary>
    /// Tiempo promedio de ocupación de mesas
    /// </summary>
    public TimeSpan TiempoPromedioOcupacion { get; set; }

    /// <summary>
    /// Zona con mayor ocupación
    /// </summary>
    public string? ZonaMayorOcupacion { get; set; }

    /// <summary>
    /// Zona con mayor disponibilidad
    /// </summary>
    public string? ZonaMayorDisponibilidad { get; set; }

    /// <summary>
    /// Próxima hora con mayor disponibilidad
    /// </summary>
    public DateTime? ProximaHoraMayorDisponibilidad { get; set; }

    /// <summary>
    /// Estadísticas por zona
    /// </summary>
    public List<EstadisticasZonaDto> PorZona { get; set; } = new();
}

/// <summary>
/// DTO para representar una reservación siguiente
/// </summary>
public class ReservacionSiguienteDto
{
    /// <summary>
    /// ID de la reservación
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de la reservación
    /// </summary>
    public DateTime FechaHora { get; set; }

    /// <summary>
    /// Número de personas
    /// </summary>
    public int NumeroPersonas { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Duración estimada en minutos
    /// </summary>
    public int DuracionEstimada { get; set; }
}

/// <summary>
/// DTO para estadísticas por zona
/// </summary>
public class EstadisticasZonaDto
{
    /// <summary>
    /// Nombre de la zona
    /// </summary>
    public string Zona { get; set; } = string.Empty;

    /// <summary>
    /// Total de mesas en la zona
    /// </summary>
    public int TotalMesas { get; set; }

    /// <summary>
    /// Mesas disponibles en la zona
    /// </summary>
    public int Disponibles { get; set; }

    /// <summary>
    /// Mesas ocupadas en la zona
    /// </summary>
    public int Ocupadas { get; set; }

    /// <summary>
    /// Mesas reservadas en la zona
    /// </summary>
    public int Reservadas { get; set; }

    /// <summary>
    /// Mesas fuera de servicio en la zona
    /// </summary>
    public int FueraDeServicio { get; set; }

    /// <summary>
    /// Porcentaje de ocupación de la zona
    /// </summary>
    public decimal PorcentajeOcupacion { get; set; }
} 