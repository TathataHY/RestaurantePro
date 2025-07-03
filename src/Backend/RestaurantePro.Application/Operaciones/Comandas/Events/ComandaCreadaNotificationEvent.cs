using MediatR;

namespace RestaurantePro.Application.Operaciones.Comandas.Events;

/// <summary>
/// Evento de aplicación para notificar cuando se crea una nueva comanda
/// Este evento se dispara desde los event handlers de dominio
/// </summary>
public class ComandaCreadaNotificationEvent : INotification
{
    /// <summary>
    /// ID de la comanda creada
    /// </summary>
    public Guid ComandaId { get; }

    /// <summary>
    /// ID de la mesa asociada
    /// </summary>
    public Guid MesaId { get; }

    /// <summary>
    /// ID del mesero que creó la comanda
    /// </summary>
    public Guid MeseroId { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ComandaCreadaNotificationEvent(Guid comandaId, Guid mesaId, Guid meseroId)
    {
        ComandaId = comandaId;
        MesaId = mesaId;
        MeseroId = meseroId;
    }
} 