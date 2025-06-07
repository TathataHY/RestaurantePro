using System;
using MediatR;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Comando para procesar un pedido completo (facturación, pago, liberación mesa)
/// </summary>
public class ProcesarPedidoCompletoCommand : IRequest<Result<ProcesarPedidoCompletoDto>>
{
    /// <summary>
    /// ID de la comanda a procesar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// Indica si se debe procesar el pago
    /// </summary>
    public bool RequierePago { get; set; }

    /// <summary>
    /// Tipo de pago (Efectivo, Tarjeta, Digital, Transferencia)
    /// </summary>
    public string TipoPago { get; set; } = string.Empty;

    /// <summary>
    /// Información adicional del pago
    /// </summary>
    public InfoPagoDto InfoPago { get; set; } = new InfoPagoDto();

    /// <summary>
    /// Indica si se debe liberar la mesa
    /// </summary>
    public bool LiberarMesa { get; set; }

    /// <summary>
    /// ID del usuario que solicita el proceso
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// ID del cliente relacionado con la factura
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Dirección de entrega (para pedidos a domicilio)
    /// </summary>
    public string? DireccionEntrega { get; set; }
    
    /// <summary>
    /// Teléfono de contacto para la entrega (para pedidos a domicilio)
    /// </summary>
    public string? TelefonoEntrega { get; set; }

    /// <summary>
    /// Observaciones adicionales para el procesamiento
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de servicio (Local, ParaLlevar, Domicilio)
    /// </summary>
    public string TipoServicio { get; set; } = "Local";
}