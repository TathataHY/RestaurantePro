using MediatR;
using RestaurantePro.Application.Features.Reportes.Dtos;
using System;

namespace RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteOcupacion
{
    public class ObtenerReporteOcupacionQuery : IRequest<ReporteOcupacionDto>
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? MesaId { get; set; }
    }
} 