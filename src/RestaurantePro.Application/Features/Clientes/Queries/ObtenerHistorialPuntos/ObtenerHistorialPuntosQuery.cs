using MediatR;
using RestaurantePro.Application.Features.Clientes.Dtos;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Clientes.Queries.ObtenerHistorialPuntos
{
    public class ObtenerHistorialPuntosQuery : IRequest<List<MovimientoPuntosDto>>
    {
        public int ClienteId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string TipoMovimiento { get; set; }
    }
} 