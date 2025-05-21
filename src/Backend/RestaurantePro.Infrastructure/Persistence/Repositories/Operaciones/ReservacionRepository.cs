using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    public class ReservacionRepository : Repository<Reservacion>, IReservacionRepository
    {
        private readonly IMesaRepository _mesaRepository;

        public ReservacionRepository(RestauranteProDbContext context, IMesaRepository mesaRepository) : base(context)
        {
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        }

        public async Task<IEnumerable<Reservacion>> ObtenerTodasAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<Reservacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorFechaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            // Filtrar por día, independientemente de la hora
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            return await _dbSet
                .Where(r => r.FechaReservacion >= fechaInicio && r.FechaReservacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerReservacionesActivasPorMesaYFechaAsync(Guid mesaId, DateTime fecha)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1).AddTicks(-1);
            
            return await _dbSet
                .Where(r => r.MesaId == mesaId &&
                           r.FechaReservacion >= fechaInicio &&
                           r.FechaReservacion <= fechaFin &&
                           (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada))
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.ClienteId == clienteId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerReservacionesPendientesPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.ClienteId == clienteId && 
                           (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada))
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorEstadoAsync(EstadoReservacion estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.Estado == estado)
                .ToListAsync(cancellationToken);
        }

        public async Task AgregarAsync(Reservacion reservacion)
        {
            await _dbSet.AddAsync(reservacion);
        }

        public Task ActualizarAsync(Reservacion reservacion)
        {
            _context.Entry(reservacion).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task EliminarAsync(Guid id)
        {
            var reservacion = await ObtenerPorIdAsync(id);
            if (reservacion != null)
            {
                _dbSet.Remove(reservacion);
            }
        }

        public async Task<bool> ExisteReservacionEnRangoHorarioAsync(Guid mesaId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            var fechaInicio = fecha.Date.Add(horaInicio);
            var fechaFin = fecha.Date.Add(horaFin);
            
            // Verificar si hay alguna reservación confirmada o pendiente que se solape con el rango de tiempo
            return await _dbSet
                .AnyAsync(r => r.MesaId == mesaId &&
                              (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente) &&
                              ((r.FechaReservacion.TimeOfDay <= horaFin && r.FechaReservacion.TimeOfDay.Add(r.DuracionEstimada) >= horaInicio)));
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorMesaAsync(Guid mesaId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.MesaId == mesaId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.FechaReservacion >= fechaInicio && r.FechaReservacion <= fechaFin)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> VerificarDisponibilidadMesaAsync(Guid mesaId, DateTime fecha, TimeSpan hora, int duracionMinutos = 90, CancellationToken cancellationToken = default)
        {
            var fechaHoraInicio = fecha.Date.Add(hora);
            var duracion = TimeSpan.FromMinutes(duracionMinutos);
            var fechaHoraFin = fechaHoraInicio.Add(duracion);
            
            // Verificar si hay alguna reservación confirmada o pendiente que se solape con el rango de tiempo
            return !await _dbSet
                .AnyAsync(r => r.MesaId == mesaId &&
                              (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente) &&
                              ((r.FechaReservacion <= fechaHoraFin && r.FechaReservacion.Add(r.DuracionEstimada) >= fechaHoraInicio)), 
                        cancellationToken);
        }

        public async Task<IEnumerable<Guid>> ObtenerMesasDisponiblesAsync(DateTime fecha, TimeSpan hora, int cantidadPersonas, int duracionMinutos = 90, CancellationToken cancellationToken = default)
        {
            // 1. Obtener todas las mesas que pueden acomodar al menos la cantidad de personas
            var mesasPosibles = await _context.Set<Mesa>()
                .Where(m => m.Capacidad >= cantidadPersonas && m.Estado == EstadoMesa.Disponible)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);
                
            // 2. Filtrar las mesas que ya tienen reservaciones en ese horario
            var fechaHoraInicio = fecha.Date.Add(hora);
            var duracion = TimeSpan.FromMinutes(duracionMinutos);
            var fechaHoraFin = fechaHoraInicio.Add(duracion);
            
            var mesasReservadas = await _dbSet
                .Where(r => mesasPosibles.Contains(r.MesaId) &&
                          (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente) &&
                          ((r.FechaReservacion <= fechaHoraFin && r.FechaReservacion.Add(r.DuracionEstimada) >= fechaHoraInicio)))
                .Select(r => r.MesaId)
                .Distinct()
                .ToListAsync(cancellationToken);
                
            // 3. Devolver las mesas disponibles (las posibles menos las reservadas)
            return mesasPosibles.Except(mesasReservadas);
        }

        public async Task<(IEnumerable<Reservacion> Reservaciones, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            var total = await query.CountAsync(cancellationToken);
            
            var reservaciones = await query
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
                
            return (reservaciones, total);
        }

        public async Task<Dictionary<DateTime, int>> ObtenerEstadisticasPorDiaAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            var reservacionesPorDia = await _dbSet
                .Where(r => r.FechaReservacion.Date >= fechaInicio.Date && r.FechaReservacion.Date <= fechaFin.Date)
                .GroupBy(r => r.FechaReservacion.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .ToListAsync(cancellationToken);
                
            return reservacionesPorDia.ToDictionary(x => x.Fecha, x => x.Cantidad);
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 