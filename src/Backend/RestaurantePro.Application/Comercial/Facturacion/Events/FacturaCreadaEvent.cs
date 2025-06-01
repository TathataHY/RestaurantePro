namespace RestaurantePro.Application.Comercial.Facturacion.Events;

/// <summary>
/// Evento que se dispara cuando se crea una nueva factura
/// </summary>
public class FacturaCreadaEvent : INotification
{
    /// <summary>
    /// ID de la factura creada
    /// </summary>
    public Guid FacturaId { get; set; }

    /// <summary>
    /// ID del cliente
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Fecha de creación de la factura
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Número de la factura
    /// </summary>
    public string NumeroFactura { get; set; } = string.Empty;

    /// <summary>
    /// Constructor
    /// </summary>
    public FacturaCreadaEvent(Guid facturaId, Guid clienteId, decimal montoTotal, string numeroFactura)
    {
        FacturaId = facturaId;
        ClienteId = clienteId;
        MontoTotal = montoTotal;
        NumeroFactura = numeroFactura;
        FechaCreacion = DateTime.UtcNow;
    }
} 