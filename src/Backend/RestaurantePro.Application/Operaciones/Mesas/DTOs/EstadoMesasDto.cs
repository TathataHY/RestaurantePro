namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

/// <summary>
/// DTO que representa el estado completo de todas las mesas
/// </summary>
public class EstadoMesasDto
{
    /// <summary>
    /// Lista de todas las mesas con su estado actual
    /// </summary>
    public List<MesaDto> Mesas { get; set; } = new();

    /// <summary>
    /// Estadísticas generales del estado de las mesas
    /// </summary>
    public EstadisticasMesasDto Estadisticas { get; set; } = new();

    /// <summary>
    /// Zona consultada (null = todas las zonas)
    /// </summary>
    public string? Zona { get; set; }

    /// <summary>
    /// Fecha y hora de la consulta
    /// </summary>
    public DateTime FechaConsulta { get; set; } = DateTime.Now;

    /// <summary>
    /// Total de mesas en el estado consultado
    /// </summary>
    public int TotalMesas => Mesas.Count;
}

/// <summary>
/// DTO para las estadísticas de mesas
/// </summary>
public class EstadisticasMesasDto
{
    /// <summary>
    /// Total de mesas disponibles
    /// </summary>
    public int MesasDisponibles { get; set; }

    /// <summary>
    /// Total de mesas ocupadas
    /// </summary>
    public int MesasOcupadas { get; set; }

    /// <summary>
    /// Total de mesas reservadas
    /// </summary>
    public int MesasReservadas { get; set; }

    /// <summary>
    /// Total de mesas fuera de servicio
    /// </summary>
    public int MesasFueraDeServicio { get; set; }

    /// <summary>
    /// Total de mesas activas
    /// </summary>
    public int MesasActivas { get; set; }

    /// <summary>
    /// Total de mesas inactivas
    /// </summary>
    public int MesasInactivas { get; set; }

    /// <summary>
    /// Porcentaje de ocupación (ocupadas + reservadas / activas)
    /// </summary>
    public decimal PorcentajeOcupacion { get; set; }

    /// <summary>
    /// Porcentaje de disponibilidad (disponibles / activas)
    /// </summary>
    public decimal PorcentajeDisponibilidad { get; set; }

    /// <summary>
    /// Capacidad total de mesas disponibles
    /// </summary>
    public int CapacidadTotalDisponible { get; set; }

    /// <summary>
    /// Capacidad total de mesas ocupadas/reservadas
    /// </summary>
    public int CapacidadTotalOcupada { get; set; }

    /// <summary>
    /// Estadísticas por zona
    /// </summary>
    public List<EstadisticasZonaDto> PorZona { get; set; } = new();

    /// <summary>
    /// Mesa con mayor tiempo de ocupación
    /// </summary>
    public MesaOcupacionDto? MesaMayorTiempoOcupacion { get; set; }

    /// <summary>
    /// Promedio de tiempo de ocupación de mesas ocupadas
    /// </summary>
    public TimeSpan? TiempoPromedioOcupacion { get; set; }
}

/// <summary>
/// DTO para estadísticas por zona
/// </summary>
public class EstadisticasZonaDto
{
    public string Zona { get; set; } = string.Empty;
    public int TotalMesas { get; set; }
    public int Disponibles { get; set; }
    public int Ocupadas { get; set; }
    public int Reservadas { get; set; }
    public int FueraDeServicio { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
}

/// <summary>
/// DTO para información de ocupación de mesa
/// </summary>
public class MesaOcupacionDto
{
    public Guid MesaId { get; set; }
    public int NumeroMesa { get; set; }
    public string Zona { get; set; } = string.Empty;
    public EstadoMesa Estado { get; set; }
    public DateTime? FechaOcupacion { get; set; }
    public TimeSpan? TiempoOcupada { get; set; }
    public Guid? ComandaId { get; set; }
    public int? NumeroComanda { get; set; }
} 