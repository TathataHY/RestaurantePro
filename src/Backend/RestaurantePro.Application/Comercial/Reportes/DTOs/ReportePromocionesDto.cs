namespace RestaurantePro.Application.Comercial.Reportes.DTOs;

public class ReportePromocionesDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalPromociones { get; set; }
    public int PromocionesActivas { get; set; }
    public int PromocionesPausadas { get; set; }
    public int PromocionesExpiradas { get; set; }
    public decimal TotalDescuentosAplicados { get; set; }
    public int TotalAplicaciones { get; set; }
    public List<PromocionEfectivaDto> PromocionesMasEfectivas { get; set; } = new();
    public List<PromocionPorTipoDto> PromocionesPorTipo { get; set; } = new();
    public DateTime GeneradoEn { get; set; } = DateTime.UtcNow;
}

public class PromocionEfectivaDto
{
    public Guid PromocionId { get; set; }
    public string NombrePromocion { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int NumeroAplicaciones { get; set; }
    public decimal TotalDescuentos { get; set; }
    public decimal DescuentoPromedio { get; set; }
    public decimal PorcentajeEfectividad { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}

public class PromocionPorTipoDto
{
    public string TipoPromocion { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal TotalDescuentos { get; set; }
    public int TotalAplicaciones { get; set; }
    public decimal EfectividadPromedio { get; set; }
} 