using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Command para procesar un pedido completo desde comanda hasta facturación
/// </summary>
public class ProcesarPedidoCompletoCommand : IRequest<Result<ProcesarPedidoCompletoDto>>
{
    /// <summary>
    /// ID de la comanda a procesar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Tipo de pago (Efectivo, Tarjeta, Transferencia, etc.)
    /// </summary>
    public string TipoPago { get; set; } = string.Empty;

    /// <summary>
    /// Indica si requiere procesamiento de pago
    /// </summary>
    public bool RequierePago { get; set; } = true;

    /// <summary>
    /// Información del pago
    /// </summary>
    public InfoPagoDto? InfoPago { get; set; }

    /// <summary>
    /// Tipo de factura
    /// </summary>
    public string? TipoFactura { get; set; }

    /// <summary>
    /// Nombre del cliente para la factura
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Identificación del cliente
    /// </summary>
    public string? IdentificacionCliente { get; set; }

    /// <summary>
    /// Dirección del cliente
    /// </summary>
    public string? DireccionCliente { get; set; }

    /// <summary>
    /// Teléfono del cliente
    /// </summary>
    public string? TelefonoCliente { get; set; }

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string? EmailCliente { get; set; }

    /// <summary>
    /// Observaciones para la factura
    /// </summary>
    public string? ObservacionesFactura { get; set; }

    /// <summary>
    /// Dirección de entrega del pedido
    /// </summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>
    /// Teléfono de contacto para la entrega
    /// </summary>
    public string? TelefonoEntrega { get; set; }

    /// <summary>
    /// ID del usuario que procesa el pedido
    /// </summary>
    public Guid UsuarioId { get; set; }
}

/// <summary>
/// DTO con información de pago
/// </summary>
public class InfoPagoDto
{
    /// <summary>
    /// Número de tarjeta (si aplica)
    /// </summary>
    public string? NumeroTarjeta { get; set; }

    /// <summary>
    /// Nombre del titular
    /// </summary>
    public string? NombreTitular { get; set; }

    /// <summary>
    /// Monto total del pago
    /// </summary>
    public decimal MontoTotal { get; set; }

    /// <summary>
    /// Moneda del pago
    /// </summary>
    public string? Moneda { get; set; } = "USD";

    /// <summary>
    /// Referencia del pago
    /// </summary>
    public string? ReferenciaPago { get; set; }

    /// <summary>
    /// Observaciones del pago
    /// </summary>
    public string? ObservacionesPago { get; set; }
} 