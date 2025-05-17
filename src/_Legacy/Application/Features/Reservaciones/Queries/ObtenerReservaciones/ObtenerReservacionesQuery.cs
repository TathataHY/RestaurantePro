using MediatR;
using RestaurantePro.Application.Features.Reservaciones.Dtos;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Reservaciones.Queries.ObtenerReservaciones
{
    public class ObtenerReservacionesQuery : IRequest<List<ReservacionDto>>
    {
        public int? MesaId { get; set; }
        public string NombreCliente { get; set; }
        public string TelefonoCliente { get; set; }
        public string EmailCliente { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public EstadoReservacion? Estado { get; set; }
        public string UsuarioId { get; set; }
    }
} 