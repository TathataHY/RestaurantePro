using RestaurantePro.Domain.Comercial.Facturacion.Enums;

namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

/// <summary>
/// DTO para representar una factura
/// </summary>
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
    
    // 🔥 PROPIEDADES BÁSICAS AGREGADAS - Estados calculados completos
    public bool EstaPagada => Estado == EstadoFactura.Pagada;
    public bool EstaPendiente => Estado == EstadoFactura.Emitida;
    public bool EsBorrador => Estado == EstadoFactura.Borrador;
    public bool EstaAnulada => Estado == EstadoFactura.Anulada;
    public bool EstaPagadaParcialmente => Estado == EstadoFactura.PagadaParcialmente;
    public bool EsRectificativa => Estado == EstadoFactura.Rectificativa;
    public bool EstaVencida => Estado == EstadoFactura.Vencida || (FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.Now && Estado != EstadoFactura.Pagada);
    public bool TieneSaldo => Saldo > 0;
    
    // 🆕 PROPIEDADES BÁSICAS NUEVAS para UI - VALORES CORREGIDOS
    public string NumeroFactura => $"F-{Numero}";
    public string TipoFacturaTexto => Tipo switch
    {
        TipoFactura.Normal => "Normal",
        TipoFactura.Fiscal => "Fiscal", 
        TipoFactura.NotaCredito => "Nota de Crédito",
        TipoFactura.Simplificada => "Simplificada",
        TipoFactura.Electronica => "Electrónica",
        _ => Tipo.ToString()
    };
} 