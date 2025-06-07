using System;
using MediatR;
using RestaurantePro.Application.Operaciones.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Comando para procesar completamente un pedido: finalizar comanda, procesar pago, generar factura,
/// acumular puntos de fidelización y liberar mesa si corresponde.
/// </summary>
public class ProcesarPedidoCompletoCommand : IRequest<Result<ProcesarPedidoCompletoDto>>
{
    /// <summary>
    /// ID de la comanda a procesar
    /// </summary>
    public Guid ComandaId { get; set; }

    /// <summary>
    /// ID del cliente (opcional)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// ID del usuario que procesa la comanda
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Tipo de pago: "Efectivo", "Tarjeta", "Transferencia", etc.
    /// </summary>
    public string TipoPago { get; set; } = "Efectivo";

    /// <summary>
    /// Indica si se requiere procesar un pago electrónico
    /// </summary>
    public bool RequierePago { get; set; } = false;

    /// <summary>
    /// Información adicional para el pago electrónico (requerido si RequierePago = true)
    /// </summary>
    public InfoPagoDto? InfoPago { get; set; }

    /// <summary>
    /// Indica si se debe liberar la mesa asociada a la comanda
    /// </summary>
    public bool LiberarMesa { get; set; } = true;

    /// <summary>
    /// Comentarios adicionales para el proceso
    /// </summary>
    public string Comentarios { get; set; } = string.Empty;

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