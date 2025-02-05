using MediatR;
using RestaurantePro.Core.Enums;
using System;

namespace RestaurantePro.Core.Events
{
    public class ComandaStatusChangedEvent : INotification
    {
        public int ComandaId { get; }
        public EstadoComanda OldStatus { get; }
        public EstadoComanda NewStatus { get; }
        public DateTime OccurredOn { get; }

        public ComandaStatusChangedEvent(int comandaId, EstadoComanda oldStatus, EstadoComanda newStatus)
        {
            ComandaId = comandaId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            OccurredOn = DateTime.UtcNow;
        }
    }
} 