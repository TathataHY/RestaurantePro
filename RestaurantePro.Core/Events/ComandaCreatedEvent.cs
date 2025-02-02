using MediatR;
using System;

namespace RestaurantePro.Core.Events
{
    public class ComandaCreatedEvent : INotification
    {
        public int ComandaId { get; }
        public int MesaId { get; }
        public DateTime FechaHora { get; }

        public ComandaCreatedEvent(int comandaId, int mesaId, DateTime fechaHora)
        {
            ComandaId = comandaId;
            MesaId = mesaId;
            FechaHora = fechaHora;
        }
    }
}