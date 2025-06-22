namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

public class PagoFacturaDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? ReferenciaPago { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
} 