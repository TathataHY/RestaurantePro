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
    public string TipoPago { get; set; }

    /// <summary>
    /// Información adicional del pago
    /// </summary>
    public InfoPagoDto InfoPago { get; set; }

    /// <summary>
    /// Indica si se debe liberar la mesa
    /// </summary>
    public bool LiberarMesa { get; set; }

    /// <summary>
    /// ID del usuario que solicita el proceso
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Observaciones adicionales para el procesamiento
    /// </summary>
    public string Observaciones { get; set; }
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