using MediatR;
using RestaurantePro.Application.Features.Pagos.Dtos;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Pagos.Queries.ObtenerPagos
{
    public class ObtenerPagosQuery : IRequest<List<PagoDto>>
    {
        public int? ComandaId { get; set; }
        public EstadoPago? Estado { get; set; }
        public MetodoPago? MetodoPago { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string UsuarioId { get; set; }
    }
} 