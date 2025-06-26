namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

public class ReporteVentasDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalVentas { get; set; }
    public int TotalTransacciones { get; set; }
    public decimal PromedioTicket { get; set; }
    public List<VentaDiariaDto> VentasPorDia { get; set; } = new();
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<SegmentoVentasDto> VentasPorSegmento { get; set; } = new();
    public DateTime GeneradoEn { get; set; } = DateTime.UtcNow;
}

public class VentaDiariaDto
{
    public DateTime Fecha { get; set; }
    public decimal TotalVentas { get; set; }
    public int NumeroTransacciones { get; set; }
    public decimal PromedioTicket { get; set; }
}

public class ProductoMasVendidoDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
}

public class SegmentoVentasDto
{
    public string Segmento { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int NumeroTransacciones { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
} 