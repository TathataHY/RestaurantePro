namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

public class ReporteFidelizacionDto
{
    public DateTime FechaGeneracion { get; set; }
    public int TotalTarjetas { get; set; }
    public int TarjetasActivas { get; set; }
    public int TarjetasInactivas { get; set; }
    public int TotalPuntosOtorgados { get; set; }
    public int TotalPuntosCanjeados { get; set; }
    public int PuntosDisponibles { get; set; }
    public List<DistribucionNivelDto> DistribucionNiveles { get; set; } = new();
    public List<TarjetaTopPuntosDto> TopTarjetasPuntos { get; set; } = new();
    public List<TarjetaTopCanjeadosDto> TopTarjetasCanjeados { get; set; } = new();
}

public class DistribucionNivelDto
{
    public string Nivel { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
    public double PuntosPromedio { get; set; }
}

public class TarjetaTopPuntosDto
{
    public Guid TarjetaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public int PuntosAcumulados { get; set; }
    public int PuntosDisponibles { get; set; }
    public int PuntosCanjeados { get; set; }
    public string NivelFidelizacion { get; set; } = string.Empty;
}

public class TarjetaTopCanjeadosDto
{
    public Guid TarjetaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public int PuntosCanjeados { get; set; }
    public int PuntosAcumulados { get; set; }
    public string NivelFidelizacion { get; set; } = string.Empty;
} 