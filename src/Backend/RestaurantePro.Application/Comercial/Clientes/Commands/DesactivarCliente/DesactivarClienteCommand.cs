using System;
using MediatR;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using r = RestaurantePro.Domain.Core.SharedKernel.Results.Result;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Command para desactivar un cliente del sistema
/// Mantiene el registro pero lo marca como inactivo para auditoría
/// </summary>
public class DesactivarClienteCommand : IRequest<r>
{
    /// <summary>
    /// ID único del cliente a desactivar
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// Motivo por el cual se desactiva el cliente
    /// </summary>
    public string MotivoDesactivacion { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que solicita la desactivación
    /// </summary>
    public string DesactivadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Indica si se debe notificar al cliente sobre la desactivación
    /// </summary>
    public bool NotificarCliente { get; set; } = false;

    /// <summary>
    /// Fecha opcional de reactivación automática
    /// </summary>
    public DateTime? FechaReactivacion { get; set; }

    /// <summary>
    /// Notas adicionales sobre la desactivación
    /// </summary>
    public string? NotasAdicionales { get; set; }

    /// <summary>
    /// Indica si se debe mantener el historial de transacciones
    /// </summary>
    public bool MantenerHistorial { get; set; } = true;

    /// <summary>
    /// Factory method para crear command con datos mínimos
    /// </summary>
    public static DesactivarClienteCommand Create(Guid clienteId, string motivo, string usuario)
    {
        return new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            DesactivadoPor = usuario,
            MantenerHistorial = true
        };
    }

    /// <summary>
    /// Factory method para desactivación temporal
    /// </summary>
    public static DesactivarClienteCommand CreateTemporal(Guid clienteId, string motivo, string usuario, DateTime fechaReactivacion)
    {
        return new DesactivarClienteCommand
        {
            ClienteId = clienteId,
            MotivoDesactivacion = motivo,
            DesactivadoPor = usuario,
            FechaReactivacion = fechaReactivacion,
            MantenerHistorial = true,
            NotificarCliente = true
        };
    }
}
