using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    public class ReservacionRepository : Repository<Reservacion>, IReservacionRepository
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly RestauranteProDbContext _context;

        public ReservacionRepository(RestauranteProDbContext context, IMesaRepository mesaRepository, ILogger<ReservacionRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
        }

        public async Task<IQueryable<Reservacion>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        {
            return _dbSet
                .Include(r => r.Mesa)
                .Include(r => r.Cliente)
                .OrderByDescending(r => r.Fecha);
        }

        public override async Task<Reservacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Mesa)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerPorFechaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            // Filtrar por día, independientemente de la hora
            var fechaBusqueda = fecha.Date;
            
            return await _dbSet
                .Where(r => r.Fecha == fechaBusqueda)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Reservacion>> ObtenerReservacionesActivasPorMesaYFechaAsync(Guid mesaId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaBusqueda = fecha.Date;
            
            return await _dbSet
                .Where(r => r.MesaId == mesaId &&
                           r.Fecha == fechaBusqueda &&
                           (r.Estado == EstadoReservacion.Pendiente || r.Estado == EstadoReservacion.Confirmada))
                .ToListAsync(cancellationToken);
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

        public new async Task AgregarAsync(Reservacion reservacion, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(reservacion, cancellationToken);
        }

        public async Task<Reservacion> CrearAsync(Reservacion reservacion, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(reservacion, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return reservacion;
        }

        public new async Task ActualizarAsync(Reservacion reservacion, CancellationToken cancellationToken = default)
        {
            _context.Entry(reservacion).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var reservacion = await ObtenerPorIdAsync(id, cancellationToken);
            if (reservacion != null)
            {
                _dbSet.Remove(reservacion);
            }
        }

        public async Task<bool> ExisteReservacionEnRangoHorarioAsync(Guid mesaId, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, CancellationToken cancellationToken = default)
        {
            var fechaBusqueda = fecha.Date;
            
            // Verificar si hay alguna reservación confirmada o pendiente que se solape con el rango de tiempo
            return await _dbSet
                .AnyAsync(r => r.MesaId == mesaId &&
                              r.Fecha == fechaBusqueda &&
                              (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente) &&
                              ((r.Hora <= horaFin && r.Hora.Add(r.DuracionEstimada) >= horaInicio)),
                              cancellationToken);
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
                .Where(r => r.Fecha >= fechaInicio.Date && r.Fecha <= fechaFin.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> VerificarDisponibilidadMesaAsync(Guid mesaId, DateTime fecha, TimeSpan hora, int duracionMinutos = 90, CancellationToken cancellationToken = default)
        {
            var fechaBusqueda = fecha.Date;
            var duracion = TimeSpan.FromMinutes(duracionMinutos);
            var horaFin = hora.Add(duracion);
            
            // Verificar si hay alguna reservación confirmada o pendiente que se solape con el rango de tiempo
            return !await _dbSet
                .AnyAsync(r => r.MesaId == mesaId &&
                              r.Fecha == fechaBusqueda &&
                              (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente) &&
                              ((r.Hora <= horaFin && r.Hora.Add(r.DuracionEstimada) >= hora)), 
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
            var duracion = TimeSpan.FromMinutes(duracionMinutos);
            
            var reservacionesEnFecha = await _dbSet
                .Where(r => mesasPosibles.Contains(r.MesaId) &&
                            r.Fecha == fecha.Date &&
                           (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente))
                .ToListAsync(cancellationToken);

            var mesasReservadas = reservacionesEnFecha
                .Where(r => r.Hora < hora.Add(duracion) && r.Hora.Add(r.DuracionEstimada) > hora)
                .Select(r => r.MesaId)
                .Distinct()
                .ToList();
                
            // 3. Devolver las mesas disponibles (las posibles menos las reservadas)
            return mesasPosibles.Except(mesasReservadas);
        }

        public new async Task<(IEnumerable<Reservacion> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            // Aplicar filtros - mostrar reservaciones que no estén canceladas
            query = query.Where(r => r.Estado != EstadoReservacion.Cancelada);
            
            // Contar total
            var total = await query.CountAsync(cancellationToken);
            
            // Paginar resultados
            var reservaciones = await query
                .OrderByDescending(r => r.FechaReservacion)
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
            
            return (Items: reservaciones, Total: total);
        }
        
        // Implementación explícita para la interfaz específica
        async Task<(IEnumerable<Reservacion> Reservaciones, int Total)> IReservacionRepository.ObtenerPaginadoAsync(
            int pagina, 
            int elementosPorPagina, 
            CancellationToken cancellationToken)
        {
            var result = await ObtenerPaginadoAsync(pagina, elementosPorPagina, cancellationToken);
            return (Reservaciones: result.Items, Total: result.Total);
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

        public override async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Reservacion?> ObtenerPorCodigoAsync(string codigoReservacion, CancellationToken cancellationToken = default)
        {
            // Como no existe la propiedad CodigoReservacion, implementamos una versión simplificada
            // que busca por NumeroReservacion o algún otro identificador similar
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Id.ToString().Contains(codigoReservacion), cancellationToken);
        }

        // Implementación de los métodos IRepository<Reservacion>
        public new async Task AgregarRangoAsync(IEnumerable<Reservacion> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public new Task<IEnumerable<Reservacion>> BuscarAsync(Func<Reservacion, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            var result = _dbSet.AsEnumerable().Where(predicado).ToList();
            return Task.FromResult<IEnumerable<Reservacion>>(result);
        }

        public new Task<bool> ExisteAsync(Func<Reservacion, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            var result = _dbSet.AsEnumerable().Any(predicado);
            return Task.FromResult(result);
        }

        public new Task<int> ContarAsync(Func<Reservacion, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            var result = _dbSet.AsEnumerable().Count(predicado);
            return Task.FromResult(result);
        }

        public new Task<Reservacion?> PrimeroODefaultAsync(Func<Reservacion, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Como Func<T, bool> no se puede traducir directamente a SQL, lo ejecutamos en memoria
            var result = _dbSet.AsEnumerable().FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public new Task<IEnumerable<Reservacion>> ObtenerPorSpecAsync(ISpecification<Reservacion> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada - debería traducir la especificación a consulta EF Core
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).ToList();
            return Task.FromResult<IEnumerable<Reservacion>>(result);
        }

        public new Task<int> ContarPorSpecAsync(ISpecification<Reservacion> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).Count();
            return Task.FromResult(result);
        }

        public new Task<Reservacion?> PrimeroODefaultPorSpecAsync(ISpecification<Reservacion> specification, CancellationToken cancellationToken = default)
        {
            // Implementación simplificada
            var query = _dbSet.AsQueryable();
            var result = SpecificationEvaluator.GetQuery(query, specification).FirstOrDefault();
            return Task.FromResult(result);
        }
    }
} 