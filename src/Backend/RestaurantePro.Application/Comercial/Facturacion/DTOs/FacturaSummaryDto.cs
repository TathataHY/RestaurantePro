namespace RestaurantePro.Application.Comercial.Facturacion.DTOs;

/// <summary>
/// DTO resumido para mostrar información básica de una factura
/// Se usa en listados y como información adicional en otros DTOs
/// </summary>
public class FacturaSummaryDto
{
    /// <summary>
    /// ID único de la factura
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la factura
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación de la factura
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de emisión de la factura
    /// </summary>
    public DateTime FechaEmision { get; set; }

    /// <summary>
    /// Monto total de la factura
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Estado actual de la factura
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Método de pago utilizado
    /// </summary>
    public string? MetodoPago { get; set; }

    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de factura
    /// </summary>
    public string? TipoFactura { get; set; }

    /// <summary>
    /// Monto pagado de la factura
    /// </summary>
    public decimal MontoPagado { get; set; }

    /// <summary>
    /// Saldo pendiente de la factura
    /// </summary>
    public decimal SaldoPendiente => MontoTotal - MontoPagado;

    /// <summary>
    /// Indica si la factura está completamente pagada
    /// </summary>
    public bool EstaPagada => SaldoPendiente <= 0;

    /// <summary>
    /// Número de items en la factura
    /// </summary>
    public int NumeroItems { get; set; }

    /// <summary>
    /// Indica si la factura tiene descuentos aplicados
    /// </summary>
    public bool TieneDescuentos { get; set; }

    /// <summary>
    /// Monto total de descuentos aplicados
    /// </summary>
    public decimal MontoDescuentos { get; set; }

    /// <summary>
    /// Usuario que creó la factura
    /// </summary>
    public string? CreadoPor { get; set; }

    /// <summary>
    /// Obtiene el estado visual para mostrar en la UI
    /// </summary>
    public string EstadoVisual
    {
        get
        {
            return Estado?.ToLower() switch
            {
                "pagada" => "success",
                "pendiente" => "warning",
                "anulada" => "danger",
                "vencida" => "danger",
                _ => "secondary"
            };
        }
    }

    /// <summary>
    /// Obtiene una descripción corta de la factura
    /// </summary>
    public string DescripcionCorta => $"#{Numero} - {NombreCliente} - {MontoTotal:C}";

    /// <summary>
    /// Indica si la factura es del día actual
    /// </summary>
    public bool EsDelDiaActual => FechaEmision.Date == DateTime.UtcNow.Date;

    /// <summary>
    /// Días desde la emisión
    /// </summary>
    public int DiasDesdeEmision => (DateTime.UtcNow.Date - FechaEmision.Date).Days;
} 