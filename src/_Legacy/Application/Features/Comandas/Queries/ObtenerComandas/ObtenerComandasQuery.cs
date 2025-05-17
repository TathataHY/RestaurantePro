using MediatR;
using RestaurantePro.Application.Features.Comandas.Dtos;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Comandas.Queries.ObtenerComandas
{
    public class ObtenerComandasQuery : IRequest<List<ComandaDto>>
    {
        public int? MesaId { get; set; }
        public EstadoComanda? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string UsuarioId { get; set; }
    }
} 