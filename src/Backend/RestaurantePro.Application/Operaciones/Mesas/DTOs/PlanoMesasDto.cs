namespace RestaurantePro.Application.Operaciones.Mesas.DTOs;

/// <summary>
/// DTO que representa el plano completo de mesas del restaurante
/// </summary>
public class PlanoMesasDto
{
    /// <summary>
    /// Lista de mesas organizadas por ubicación
    /// </summary>
    public List<MesaPlanoDto> Mesas { get; set; } = new();

    /// <summary>
    /// Estadísticas del plano
    /// </summary>
    public EstadisticasPlanoDto Estadisticas { get; set; } = new();

    /// <summary>
    /// Fecha y hora de generación del plano
    /// </summary>
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
}

/// <summary>
/// DTO para una mesa en el plano
/// </summary>
public class MesaPlanoDto
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public int Capacidad { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? ClienteId { get; set; }
    public string? NombreCliente { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaReserva { get; set; }
    public TimeSpan? HoraReserva { get; set; }
}

/// <summary>
/// DTO para estadísticas del plano
/// </summary>
public class EstadisticasPlanoDto
{
    public int TotalMesas { get; set; }
    public int MesasDisponibles { get; set; }
    public int MesasOcupadas { get; set; }
    public int MesasReservadas { get; set; }
    public int MesasMantenimiento { get; set; }
    public int CapacidadTotal { get; set; }
    public int CapacidadDisponible { get; set; }
} 