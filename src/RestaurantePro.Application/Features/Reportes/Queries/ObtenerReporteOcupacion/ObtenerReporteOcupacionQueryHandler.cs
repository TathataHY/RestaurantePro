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

namespace RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteOcupacion
{
    public class ObtenerReporteOcupacionQueryHandler : IRequestHandler<ObtenerReporteOcupacionQuery, ReporteOcupacionDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerReporteOcupacionQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteOcupacionDto> Handle(ObtenerReporteOcupacionQuery request, CancellationToken cancellationToken)
        {
            // Asegurarse de que la fecha fin incluya todo el día
            var fechaFin = request.FechaFin.Date.AddDays(1).AddTicks(-1);
            
            // Obtener todas las mesas activas
            var mesas = await _context.Mesas
                .Where(m => m.Activa)
                .ToListAsync(cancellationToken);

            if (request.MesaId.HasValue)
            {
                mesas = mesas.Where(m => m.Id == request.MesaId.Value).ToList();
            }

            // Obtener el historial de ocupación de mesas
            var historialOcupacion = await _context.HistorialEstadosMesa
                .Where(h => h.FechaHora >= request.FechaInicio.Date && 
                           h.FechaHora <= fechaFin &&
                           (h.EstadoNuevo == EstadoMesa.Ocupada || h.EstadoAnterior == EstadoMesa.Ocupada))
                .OrderBy(h => h.MesaId)
                .ThenBy(h => h.FechaHora)
                .ToListAsync(cancellationToken);

            // Calcular ocupaciones por mesa
            var ocupacionesPorMesa = new List<OcupacionPorMesaDto>();
            foreach (var mesa in mesas)
            {
                var historialMesa = historialOcupacion
                    .Where(h => h.MesaId == mesa.Id)
                    .OrderBy(h => h.FechaHora)
                    .ToList();
                
                // Agrupar por "sesiones" de ocupación (desde Disponible->Ocupada hasta Ocupada->Disponible)
                var sesionesOcupacion = new List<(DateTime inicio, DateTime fin)>();
                DateTime? inicioOcupacion = null;
                
                foreach (var registro in historialMesa)
                {
                    if (registro.EstadoAnterior != EstadoMesa.Ocupada && registro.EstadoNuevo == EstadoMesa.Ocupada)
                    {
                        // Inicio de ocupación
                        inicioOcupacion = registro.FechaHora;
                    }
                    else if (registro.EstadoAnterior == EstadoMesa.Ocupada && registro.EstadoNuevo != EstadoMesa.Ocupada)
                    {
                        // Fin de ocupación
                        if (inicioOcupacion.HasValue)
                        {
                            sesionesOcupacion.Add((inicioOcupacion.Value, registro.FechaHora));
                            inicioOcupacion = null;
                        }
                    }
                }
                
                // Si hay una ocupación sin cerrar, asumimos que termina ahora
                if (inicioOcupacion.HasValue)
                {
                    sesionesOcupacion.Add((inicioOcupacion.Value, DateTime.Now));
                }

                // Calcular tiempo promedio de ocupación
                var totalMinutos = sesionesOcupacion.Sum(s => (s.fin - s.inicio).TotalMinutes);
                var tiempoPromedio = sesionesOcupacion.Count > 0 
                    ? TimeSpan.FromMinutes(totalMinutos / sesionesOcupacion.Count) 
                    : TimeSpan.Zero;

                // Calcular porcentaje de ocupación (tiempo ocupada / tiempo total del período)
                var totalMinutosPeriodo = (fechaFin - request.FechaInicio.Date).TotalMinutes;
                var porcentajeOcupacion = totalMinutosPeriodo > 0 
                    ? (decimal)(totalMinutos / totalMinutosPeriodo * 100) 
                    : 0;

                ocupacionesPorMesa.Add(new OcupacionPorMesaDto
                {
                    MesaId = mesa.Id,
                    NumeroMesa = mesa.Numero,
                    Capacidad = mesa.Capacidad,
                    VecesOcupada = sesionesOcupacion.Count,
                    TiempoPromedioOcupacion = tiempoPromedio,
                    PorcentajeOcupacion = Math.Round(porcentajeOcupacion, 2)
                });
            }

            // Calcular ocupación por día
            var diasDelPeriodo = Enumerable.Range(0, (int)(fechaFin.Date - request.FechaInicio.Date).TotalDays + 1)
                .Select(offset => request.FechaInicio.Date.AddDays(offset))
                .ToList();

            var ocupacionPorDia = new List<OcupacionPorDiaDto>();
            foreach (var fecha in diasDelPeriodo)
            {
                var historialDia = historialOcupacion
                    .Where(h => h.FechaHora.Date == fecha)
                    .ToList();
                
                // Contar mesas que estuvieron ocupadas ese día
                var mesasOcupadas = new HashSet<int>();
                foreach (var registro in historialDia)
                {
                    if (registro.EstadoAnterior != EstadoMesa.Ocupada && registro.EstadoNuevo == EstadoMesa.Ocupada)
                    {
                        mesasOcupadas.Add(registro.MesaId);
                    }
                }

                // Calcular porcentaje de ocupación del día
                var porcentajeOcupacion = mesas.Count > 0 
                    ? (decimal)mesasOcupadas.Count / mesas.Count * 100 
                    : 0;

                ocupacionPorDia.Add(new OcupacionPorDiaDto
                {
                    Fecha = fecha,
                    TotalOcupaciones = mesasOcupadas.Count,
                    PorcentajeOcupacion = Math.Round(porcentajeOcupacion, 2)
                });
            }

            // Calcular horas pico (por hora del día)
            var ocupacionesPorHora = new Dictionary<int, int>();
            foreach (var registro in historialOcupacion)
            {
                if (registro.EstadoAnterior != EstadoMesa.Ocupada && registro.EstadoNuevo == EstadoMesa.Ocupada)
                {
                    var hora = registro.FechaHora.Hour;
                    if (!ocupacionesPorHora.ContainsKey(hora))
                    {
                        ocupacionesPorHora[hora] = 0;
                    }
                    ocupacionesPorHora[hora]++;
                }
            }

            var totalOcupaciones = ocupacionesPorHora.Values.Sum();
            var horasPico = ocupacionesPorHora
                .Select(kv => new HoraPicoDto
                {
                    Hora = kv.Key,
                    RangoHoras = $"{kv.Key:00}:00 - {kv.Key:00}:59",
                    TotalOcupaciones = kv.Value,
                    PorcentajeOcupacion = totalOcupaciones > 0 
                        ? Math.Round((decimal)kv.Value / totalOcupaciones * 100, 2) 
                        : 0
                })
                .OrderByDescending(h => h.TotalOcupaciones)
                .ToList();

            // Construir el reporte
            return new ReporteOcupacionDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                TotalMesas = mesas.Count,
                TotalOcupaciones = ocupacionesPorMesa.Sum(o => o.VecesOcupada),
                PromedioOcupacionDiaria = ocupacionPorDia.Count > 0 
                    ? Math.Round(ocupacionPorDia.Average(o => o.PorcentajeOcupacion), 2) 
                    : 0,
                TiempoPromedioOcupacion = ocupacionesPorMesa.Count > 0 && ocupacionesPorMesa.Sum(o => o.VecesOcupada) > 0
                    ? TimeSpan.FromMinutes(ocupacionesPorMesa.Sum(o => o.TiempoPromedioOcupacion.TotalMinutes * o.VecesOcupada) / ocupacionesPorMesa.Sum(o => o.VecesOcupada))
                    : TimeSpan.Zero,
                OcupacionPorDia = ocupacionPorDia,
                OcupacionPorMesa = ocupacionesPorMesa,
                HorasPico = horasPico
            };
        }
    }
} 