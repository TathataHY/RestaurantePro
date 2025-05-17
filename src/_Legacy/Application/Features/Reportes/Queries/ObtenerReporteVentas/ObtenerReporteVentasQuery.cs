using MediatR;
using RestaurantePro.Application.Features.Reportes.Dtos;
using System;

namespace RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteVentas
{
    public class ObtenerReporteVentasQuery : IRequest<ReporteVentasDto>
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? CategoriaId { get; set; }
        public int? TopProductos { get; set; } = 10; // Por defecto mostrar los 10 productos más vendidos
    }
} 