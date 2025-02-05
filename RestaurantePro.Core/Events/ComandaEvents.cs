using MediatR;
using System;
using RestaurantePro.Core.DTOs.Comanda;
using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.Events
{
    public abstract class ComandaEvent : INotification
    {
        public int ComandaId { get; }
        public DateTime Timestamp { get; }

        protected ComandaEvent(int comandaId)
        {
            ComandaId = comandaId;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class ComandaEstadoCambiadoEvent : ComandaEvent
    {
        public EstadoComanda EstadoAnterior { get; }
        public EstadoComanda NuevoEstado { get; }
        public string Usuario { get; }

        public ComandaEstadoCambiadoEvent(
            int comandaId,
            EstadoComanda estadoAnterior,
            EstadoComanda nuevoEstado,
            string usuario) : base(comandaId)
        {
            EstadoAnterior = estadoAnterior;
            NuevoEstado = nuevoEstado;
            Usuario = usuario;
        }
    }

    public class ComandaDetalleAgregadoEvent : ComandaEvent
    {
        public ComandaDetalleDto Detalle { get; }

        public ComandaDetalleAgregadoEvent(int comandaId, ComandaDetalleDto detalle)
            : base(comandaId)
        {
            Detalle = detalle;
        }
    }
}