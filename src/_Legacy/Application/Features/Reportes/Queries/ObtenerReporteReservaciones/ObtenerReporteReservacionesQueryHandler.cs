using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Reportes.Dtos;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteReservaciones
{
    public class ObtenerReporteReservacionesQueryHandler : IRequestHandler<ObtenerReporteReservacionesQuery, ReporteReservacionesDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerReporteReservacionesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteReservacionesDto> Handle(ObtenerReporteReservacionesQuery request, CancellationToken cancellationToken)
        {
            var fechaFin = request.FechaFin.Date.AddDays(1).AddTicks(-1);
            
            var query = _context.Reservaciones
                .Include(r => r.Mesa)
                .Where(r => r.FechaReservacion >= request.FechaInicio.Date && 
                           r.FechaReservacion <= fechaFin);

            if (request.MesaId.HasValue)
            {
                query = query.Where(r => r.MesaId == request.MesaId.Value);
            }

            var reservaciones = await query.ToListAsync(cancellationToken);

            // Estadísticas generales
            var totalReservaciones = reservaciones.Count;
            var completadas = reservaciones.Count(r => r.Estado == EstadoReservacion.Completada);
            var canceladas = reservaciones.Count(r => r.Estado == EstadoReservacion.Cancelada);
            var noShow = reservaciones.Count(r => r.Estado == EstadoReservacion.NoShow);

            // Calcular porcentajes
            var porcentajeCompletadas = totalReservaciones > 0 ? (decimal)completadas / totalReservaciones * 100 : 0;
            var porcentajeCanceladas = totalReservaciones > 0 ? (decimal)canceladas / totalReservaciones * 100 : 0;
            var porcentajeNoShow = totalReservaciones > 0 ? (decimal)noShow / totalReservaciones * 100 : 0;

            // Reservaciones por día
            var reservacionesPorDia = reservaciones
                .GroupBy(r => r.FechaReservacion.Date)
                .Select(g => new ReservacionPorDiaDto
                {
                    Fecha = g.Key,
                    TotalReservaciones = g.Count(),
                    Completadas = g.Count(r => r.Estado == EstadoReservacion.Completada),
                    Canceladas = g.Count(r => r.Estado == EstadoReservacion.Cancelada),
                    NoShow = g.Count(r => r.Estado == EstadoReservacion.NoShow)
                })
                .OrderBy(r => r.Fecha)
                .ToList();

            // Reservaciones por mesa
            var reservacionesPorMesa = reservaciones
                .GroupBy(r => new { r.MesaId, r.Mesa.Numero, r.Mesa.Capacidad })
                .Select(g => new ReservacionPorMesaDto
                {
                    MesaId = g.Key.MesaId,
                    NumeroMesa = g.Key.Numero,
                    Capacidad = g.Key.Capacidad,
                    TotalReservaciones = g.Count(),
                    Completadas = g.Count(r => r.Estado == EstadoReservacion.Completada),
                    Canceladas = g.Count(r => r.Estado == EstadoReservacion.Cancelada),
                    NoShow = g.Count(r => r.Estado == EstadoReservacion.NoShow)
                })
                .OrderByDescending(r => r.TotalReservaciones)
                .ToList();

            // Horas populares para reservaciones
            var horasPopulares = reservaciones
                .GroupBy(r => r.FechaReservacion.Hour)
                .Select(g => new HoraPopularReservacionDto
                {
                    Hora = g.Key,
                    RangoHoras = $"{g.Key:00}:00 - {g.Key:00}:59",
                    TotalReservaciones = g.Count(),
                    Porcentaje = totalReservaciones > 0 ? (decimal)g.Count() / totalReservaciones * 100 : 0
                })
                .OrderByDescending(h => h.TotalReservaciones)
                .ToList();

            // Construir el reporte completo
            return new ReporteReservacionesDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                TotalReservaciones = totalReservaciones,
                ReservacionesCompletadas = completadas,
                ReservacionesCanceladas = canceladas,
                ReservacionesNoShow = noShow,
                PorcentajeCompletadas = Math.Round(porcentajeCompletadas, 2),
                PorcentajeCanceladas = Math.Round(porcentajeCanceladas, 2),
                PorcentajeNoShow = Math.Round(porcentajeNoShow, 2),
                ReservacionesPorDia = reservacionesPorDia,
                ReservacionesPorMesa = reservacionesPorMesa,
                HorasPopularesReservacion = horasPopulares
            };
        }
    }
} 