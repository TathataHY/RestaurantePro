using MediatR;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Features.Inventario.Dtos;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerSugerenciasCompra
{
    public class ObtenerSugerenciasCompraQuery : IRequest<List<SugerenciaCompraDto>>
    {
        /// <summary>
        /// Periodo histórico a considerar para calcular el consumo promedio (días)
        /// </summary>
        public int PeriodoHistoricoDias { get; set; } = 30;

        /// <summary>
        /// Periodo futuro a proyectar para calcular necesidades (días)
        /// </summary>
        public int PeriodoProyeccionDias { get; set; } = 7;

        /// <summary>
        /// Porcentaje de reserva adicional sobre lo proyectado (margen de seguridad)
        /// </summary>
        public decimal PorcentajeReserva { get; set; } = 20;

        /// <summary>
        /// Considerar solo ingredientes bajo su nivel mínimo
        /// </summary>
        public bool SoloBajoMinimo { get; set; } = true;
    }
} 