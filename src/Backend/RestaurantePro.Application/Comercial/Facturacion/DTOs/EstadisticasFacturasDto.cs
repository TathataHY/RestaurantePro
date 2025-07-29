namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

/// <summary>
/// DTO para estadísticas de facturas
/// </summary>
public class EstadisticasFacturasDto
{
    public DateTime Fecha { get; set; }
    public int TotalFacturas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal PromedioTicket { get; set; }
    public int FacturasPagadas { get; set; }
    public int FacturasPendientes { get; set; }
    public int FacturasAnuladas { get; set; }
    public Dictionary<string, int> FacturasPorEstado { get; set; } = new();
} 