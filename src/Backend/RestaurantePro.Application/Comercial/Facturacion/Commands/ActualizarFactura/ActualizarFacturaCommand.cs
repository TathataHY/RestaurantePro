namespace RestaurantePro.Application.Comercial.Facturacion.Commands.ActualizarFactura;

public class ActualizarFacturaCommand
{
    public Guid Id { get; set; }
    public string? NombreCliente { get; set; }
    public string? IdentificacionFiscal { get; set; }
    public string? DireccionCliente { get; set; }
    public string? EmailCliente { get; set; }
    public string? Observaciones { get; set; }
    public int? DiasCredito { get; set; }
} 