using System;
using System.Collections.Generic;
using MediatR;
using RestaurantePro.Application.Common.Results;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha
{
    /// <summary>
    /// Consulta para obtener las reservaciones por fecha
    /// </summary>
    public class ObtenerReservacionesPorFechaQuery : IRequest<Result<List<ReservacionDto>>>
    {
        /// <summary>
        /// Fecha para la que se quieren obtener las reservaciones
        /// </summary>
        public DateTime Fecha { get; set; }
    }
} 