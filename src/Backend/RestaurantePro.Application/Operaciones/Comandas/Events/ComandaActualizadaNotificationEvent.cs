using MediatR;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Events;

/// <summary>
/// Evento de aplicación para notificar cuando se actualiza el estado de una comanda
/// Este evento se dispara desde los event handlers de dominio
/// </summary>
public class ComandaActualizadaNotificationEvent : INotification
{
    /// <summary>
    /// ID de la comanda
    /// </summary>
    public Guid ComandaId { get; }

    /// <summary>
    /// Estado anterior de la comanda
    /// </summary>
    public EstadoComanda EstadoAnterior { get; }

    /// <summary>
    /// Nuevo estado de la comanda
    /// </summary>
    public EstadoComanda NuevoEstado { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ComandaActualizadaNotificationEvent(Guid comandaId, EstadoComanda estadoAnterior, EstadoComanda nuevoEstado)
    {
        ComandaId = comandaId;
        EstadoAnterior = estadoAnterior;
        NuevoEstado = nuevoEstado;
    }
} 