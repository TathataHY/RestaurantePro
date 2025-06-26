namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

public class ReporteClientesDto
{
    public DateTime FechaGeneracion { get; set; }
    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int ClientesInactivos { get; set; }
    public double PorcentajeActivos { get; set; }
    public List<SegmentoEdadDto> ClientesPorEdad { get; set; } = new();
    public List<ClienteTopPuntosDto> TopClientesPuntos { get; set; } = new();
    public List<ClienteTopVisitasDto> TopClientesVisitas { get; set; } = new();
    public List<SegmentoClienteDto> DistribucionSegmento { get; set; } = new();
    public List<NuevosClientesMesDto> NuevosClientesPorMes { get; set; } = new();
}

public class SegmentoEdadDto
{
    public string GrupoEdad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class ClienteTopPuntosDto
{
    public Guid ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int PuntosAcumulados { get; set; }
    public int CantidadVisitas { get; set; }
}

public class ClienteTopVisitasDto
{
    public Guid ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int CantidadVisitas { get; set; }
    public int PuntosAcumulados { get; set; }
}

public class SegmentoClienteDto
{
    public string Segmento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class NuevosClientesMesDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public int Cantidad { get; set; }
} 