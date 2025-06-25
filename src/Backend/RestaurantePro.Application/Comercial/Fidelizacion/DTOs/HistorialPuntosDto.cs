namespace RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

/// <summary>
/// DTO para representar un movimiento en el historial de puntos
/// </summary>
public class HistorialPuntosDto
{
    public Guid Id { get; set; }
    public Guid TarjetaFidelizacionId { get; set; }
    public int Puntos { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaMovimiento { get; set; }
    public decimal? MontoTransaccion { get; set; }
    public string? Referencia { get; set; }
    public Guid UsuarioId { get; set; }
}

/// <summary>
/// DTO para representar las estadísticas de una tarjeta de fidelización
/// </summary>
public class EstadisticasTarjetaDto
{
    public Guid TarjetaFidelizacionId { get; set; }
    public int PuntosActuales { get; set; }
    public int PuntosAcumulados { get; set; }
    public int PuntosCanjeados { get; set; }
    public int TotalMovimientos { get; set; }
    public decimal MontoTotalGastado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimaActividad { get; set; }
    public string NivelActual { get; set; } = string.Empty;
    public decimal MultiplicadorActual { get; set; }
} 