using MediatR;
using RestaurantePro.Application.Features.Inventario.Dtos;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerEstadisticasInventario
{
    public class ObtenerEstadisticasInventarioQuery : IRequest<EstadisticasInventarioDto>
    {
        /// <summary>
        /// Periodo para calcular las estadísticas en días (por defecto 30 días)
        /// </summary>
        public int PeriodoDias { get; set; } = 30;
    }
} 