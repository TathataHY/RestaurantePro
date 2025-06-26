namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

public class ReporteProductosDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalProductosVendidos { get; set; }
    public decimal TotalVentasProductos { get; set; }
    public List<ProductoVendidoDto> ProductosMasVendidos { get; set; } = new();
    public List<ProductoVendidoDto> ProductosMenosVendidos { get; set; } = new();
    public List<ProductoRentabilidadDto> ProductosPorRentabilidad { get; set; } = new();
    public DateTime GeneradoEn { get; set; } = DateTime.UtcNow;
}

public class ProductoVendidoDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal PorcentajeDelTotal { get; set; }
}

public class ProductoRentabilidadDto
{
    public Guid ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public decimal CostoPromedio { get; set; }
    public decimal PrecioVentaPromedio { get; set; }
    public decimal MargenBruto { get; set; }
    public decimal PorcentajeRentabilidad { get; set; }
} 