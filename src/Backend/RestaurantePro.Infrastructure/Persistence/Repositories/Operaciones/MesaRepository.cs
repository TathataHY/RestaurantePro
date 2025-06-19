using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Implementación del repositorio de mesas
    /// </summary>
    public class MesaRepository : Repository<Mesa>, IMesaRepository
    {
        private readonly RestauranteProDbContext _restauranteProDbContext;
        private readonly IDateTimeService _dateTimeService;

        public MesaRepository(RestauranteProDbContext context, ILogger<MesaRepository> logger, IDateTimeService dateTimeService)
            : base(context, logger)
        {
            _restauranteProDbContext = context;
            _dateTimeService = dateTimeService;
        }

        /// <summary>
        /// Obtiene todas las mesas
        /// </summary>
        public async Task<IEnumerable<Mesa>> ObtenerTodasAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Obtiene una mesa por su ID
        /// </summary>
        public async Task<Mesa> ObtenerPorIdAsync(Guid id)
        {
            var mesa = await _dbSet.FindAsync(id);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Mesa con ID {id} no encontrada");
            }
            return mesa;
        }

        /// <summary>
        /// Obtiene una mesa por su número
        /// </summary>
        public async Task<Mesa> ObtenerPorNumeroAsync(int numero)
        {
            var mesa = await _dbSet.FirstOrDefaultAsync(m => m.Numero == numero);
            if (mesa == null)
            {
                throw new KeyNotFoundException($"Mesa con número {numero} no encontrada");
            }
            return mesa;
        }

        /// <summary>
        /// Busca mesas por ubicación
        /// </summary>
        public async Task<IEnumerable<Mesa>> BuscarPorUbicacionAsync(string ubicacion)
        {
            return await _dbSet
                .Where(m => m.Ubicacion.Contains(ubicacion))
                .ToListAsync();
        }

        /// <summary>
        /// Agrega una nueva mesa
        /// </summary>
        public async Task AgregarAsync(Mesa mesa)
        {
            await _dbSet.AddAsync(mesa);
        }

        /// <summary>
        /// Actualiza una mesa existente
        /// </summary>
        public Task ActualizarAsync(Mesa mesa)
        {
            _dbContext.Entry(mesa).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Elimina una mesa
        /// </summary>
        public async Task EliminarAsync(Guid id)
        {
            var mesa = await ObtenerPorIdAsync(id);
            _dbSet.Remove(mesa);
        }

        /// <summary>
        /// Obtiene el número de comensales actuales (suma de capacidades de mesas ocupadas)
        /// </summary>
        public async Task<int> ObtenerTotalComensalesActualesAsync()
        {
            return await _dbSet
                .Where(m => m.Estado == EstadoMesa.Ocupada)
                .SumAsync(m => m.Capacidad);
        }

        /// <summary>
        /// Obtiene la lista de mesas disponibles
        /// </summary>
        public async Task<IEnumerable<Mesa>> ObtenerMesasDisponiblesAsync()
        {
            return await _dbSet
                .Where(m => m.Estado == EstadoMesa.Disponible)
                .OrderBy(m => m.Numero)
                .ToListAsync();
        }

        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        public async Task GuardarCambiosAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Busca la mejor mesa disponible para los criterios especificados
        /// </summary>
        public async Task<Mesa?> BuscarMejorMesaAsync(int numeroPersonas, string? areaPreferida = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(m => m.Estado == EstadoMesa.Disponible && m.Capacidad >= numeroPersonas);
            
            if (!string.IsNullOrEmpty(areaPreferida))
            {
                query = query.Where(m => m.Ubicacion == areaPreferida);
            }
            
            return await query
                .OrderBy(m => m.Capacidad) // Primero la mesa más pequeña que cumpla los requisitos
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Busca mesas con criterios específicos
        /// </summary>
        public async Task<IEnumerable<Mesa>> BuscarMesasAsync(int? capacidadMinima = null, string? area = null, bool? disponible = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (capacidadMinima.HasValue)
            {
                query = query.Where(m => m.Capacidad >= capacidadMinima.Value);
            }
            
            if (!string.IsNullOrEmpty(area))
            {
                query = query.Where(m => m.Ubicacion == area);
            }
            
            if (disponible.HasValue)
            {
                EstadoMesa estado = disponible.Value ? EstadoMesa.Disponible : EstadoMesa.Ocupada;
                query = query.Where(m => m.Estado == estado);
            }
            
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene mesas por capacidad específica
        /// </summary>
        public async Task<IEnumerable<Mesa>> ObtenerMesasPorCapacidadAsync(int capacidad, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.Capacidad >= capacidad)
                .OrderBy(m => m.Capacidad)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Verifica si una mesa específica está disponible
        /// </summary>
        public async Task<bool> VerificarDisponibilidadAsync(Guid mesaId, CancellationToken cancellationToken = default)
        {
            var mesa = await _dbSet.FindAsync(new object[] { mesaId }, cancellationToken);
            return mesa != null && mesa.Estado == EstadoMesa.Disponible;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Mesa>> ObtenerMesasSuciaPorAntiguedad(int minutosAntiguedad)
        {
            var fechaLimite = _dateTimeService.Now.AddMinutes(-minutosAntiguedad);

            var mesas = await _restauranteProDbContext.Mesas
                .Where(m => m.Estado == EstadoMesa.EnLimpieza && m.FechaActualizacion < fechaLimite)
                .ToListAsync();
                
            return mesas;
        }

        #region Implementación IRepository<Mesa>

        public override async Task<IEnumerable<Mesa>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await ObtenerTodasAsync();
        }

        public override async Task<IEnumerable<Mesa>> AgregarRangoAsync(IEnumerable<Mesa> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        public override async Task<Mesa> EliminarAsync(Mesa entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
            return entity;
        }

        public override async Task<Mesa> EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await ObtenerPorIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await EliminarAsync(entity, cancellationToken);
            }
            return entity;
        }

        public override async Task<(IEnumerable<Mesa> Items, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var total = await _dbSet.CountAsync(cancellationToken);
            var items = await _dbSet
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (Items: items, Total: total);
        }

        public new Task<IEnumerable<Mesa>> BuscarAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Mesa>>(_dbSet.Where(predicado).ToList());
        }

        public new Task<bool> ExisteAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Any(predicado));
        }

        public new Task<int> ContarAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Count(predicado));
        }

        public new Task<Mesa?> PrimeroODefaultAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.FirstOrDefault(predicado));
        }

        public new async Task<IEnumerable<Mesa>> ObtenerPorSpecAsync(
            ISpecification<Mesa> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.ToListAsync(cancellationToken);
        }

        public new async Task<int> ContarPorSpecAsync(
            ISpecification<Mesa> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.CountAsync(cancellationToken);
        }

        public new async Task<Mesa?> PrimeroODefaultPorSpecAsync(
            ISpecification<Mesa> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.FirstOrDefaultAsync(cancellationToken);
        }

        public override async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        Task<Mesa?> IRepository<Mesa>.ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return ObtenerPorIdAsync(id, cancellationToken);
        }

        Task IRepository<Mesa>.AgregarAsync(Mesa entity, CancellationToken cancellationToken)
        {
            return AgregarAsync(entity);
        }

        Task IRepository<Mesa>.ActualizarAsync(Mesa entity, CancellationToken cancellationToken)
        {
            return ActualizarAsync(entity);
        }

        #endregion
    }
} 