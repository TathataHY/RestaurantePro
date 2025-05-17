using MediatR;
using RestaurantePro.Application.Features.Clientes.Dtos;
using System.Collections.Generic;

namespace RestaurantePro.Application.Features.Clientes.Queries.ObtenerClientes
{
    public class ObtenerClientesQuery : IRequest<List<ClienteDto>>
    {
        public string Busqueda { get; set; }
        public bool? Activos { get; set; }
        public int? NivelFidelizacionId { get; set; }
        public int? PuntosMinimos { get; set; }
        public bool? ConTarjeta { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
    }
} 