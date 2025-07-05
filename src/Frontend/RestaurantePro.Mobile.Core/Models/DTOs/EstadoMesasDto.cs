namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO que representa el estado completo de todas las mesas para aplicación móvil
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

    /// <summary>
    /// Mesas disponibles
    /// </summary>
    public List<MesaDto> MesasDisponibles => 
        Mesas.Where(m => m.Disponible).ToList();

    /// <summary>
    /// Mesas ocupadas
    /// </summary>
    public List<MesaDto> MesasOcupadas => 
        Mesas.Where(m => m.Ocupada).ToList();

    /// <summary>
    /// Mesas reservadas
    /// </summary>
    public List<MesaDto> MesasReservadas => 
        Mesas.Where(m => m.Reservada).ToList();
}

/// <summary>
/// DTO para las estadísticas de mesas móvil
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
    /// Promedio de tiempo de ocupación de mesas ocupadas
    /// </summary>
    public TimeSpan? TiempoPromedioOcupacion { get; set; }

    /// <summary>
    /// Descripción del estado general para la UI
    /// </summary>
    public string DescripcionEstado
    {
        get
        {
            if (PorcentajeOcupacion >= 90) return "Muy ocupado";
            if (PorcentajeOcupacion >= 70) return "Ocupado";
            if (PorcentajeOcupacion >= 40) return "Moderado";
            return "Tranquilo";
        }
    }

    /// <summary>
    /// Color asociado al estado general para la UI
    /// </summary>
    public string ColorEstado
    {
        get
        {
            if (PorcentajeOcupacion >= 90) return "#F44336"; // Rojo
            if (PorcentajeOcupacion >= 70) return "#FF9800"; // Naranja
            if (PorcentajeOcupacion >= 40) return "#FFC107"; // Amarillo
            return "#4CAF50"; // Verde
        }
    }
}

/// <summary>
/// DTO para estadísticas por zona móvil
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

    /// <summary>
    /// Descripción breve del estado de la zona
    /// </summary>
    public string EstadoZona => PorcentajeOcupacion switch
    {
        >= 80 => "Llena",
        >= 60 => "Ocupada", 
        >= 30 => "Moderada",
        _ => "Libre"
    };
} 