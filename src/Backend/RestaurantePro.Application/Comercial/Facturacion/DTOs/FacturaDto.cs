namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

public class FacturaDto : BaseDto
{
    public string Numero { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public string? NombreCliente { get; set; }
    public Guid ComandaId { get; set; }
    public int NumeroComanda { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public EstadoFactura Estado { get; set; }
    public string EstadoTexto => Estado.ToString();
    public TipoFactura Tipo { get; set; }
    public string TipoTexto => Tipo.ToString();
    
    // Totales
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Descuentos { get; set; }
    public decimal Total { get; set; }
    
    // Información de pago
    public decimal MontoPagado { get; set; }
    public decimal Saldo => Total - MontoPagado;
    public DateTime? FechaPago { get; set; }
    public string? MetodoPago { get; set; }
    public string? ReferenciaPago { get; set; }
    
    // Estados calculados
    public bool EstaPagada => Estado == EstadoFactura.Pagada;
    // TODO: Verificar valores correctos del enum EstadoFactura
    // public bool EstaPendiente => Estado == EstadoFactura.Pendiente;
    public bool EstaVencida => FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.Now && Estado != EstadoFactura.Pagada;
    public bool TieneSaldo => Saldo > 0;
} 