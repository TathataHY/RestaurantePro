namespace RestaurantePro.Application.Comercial.Facturacion.Commands.RegistrarPagoFactura;

/// <summary>
/// Command para registrar un pago en una factura específica
/// </summary>
public class RegistrarPagoFacturaCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID de la factura a la que se registrará el pago
    /// </summary>
    public Guid FacturaId { get; set; }
    
    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public string MetodoPago { get; set; }
    
    /// <summary>
    /// Referencia del pago (número de tarjeta, transferencia, etc.)
    /// </summary>
    public string? ReferenciaPago { get; set; }
    
    /// <summary>
    /// Observaciones adicionales del pago
    /// </summary>
    public string? Observaciones { get; set; }
} 