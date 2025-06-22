namespace RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;

public class RegistrarPagoFacturaCommand
{
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string? ReferenciaPago { get; set; }
    public DateTime? FechaPago { get; set; }
    public string? Observaciones { get; set; }
} 