using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Implementación del repositorio de mesas
    /// </summary>
    public class MesaRepository : Repository<Mesa>, IMesaRepository
    {
        private readonly RestauranteProDbContext _restauranteProDbContext;

        public MesaRepository(RestauranteProDbContext context, ILogger<MesaRepository> logger)
            : base(context, logger)
        {
            _restauranteProDbContext = context;
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
        public new async Task<Mesa> ObtenerPorIdAsync(Guid id)
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
        public new async Task AgregarAsync(Mesa mesa)
        {
            await _dbSet.AddAsync(mesa);
        }

        /// <summary>
        /// Actualiza una mesa existente
        /// </summary>
        public new Task ActualizarAsync(Mesa mesa)
        {
            _dbContext.Entry(mesa).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Elimina una mesa
        /// </summary>
        public new async Task EliminarAsync(Guid id)
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

        #region Implementación IRepository<Mesa>

        public Task<IEnumerable<Mesa>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return ObtenerTodasAsync();
        }

        public async Task AgregarRangoAsync(IEnumerable<Mesa> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public async Task EliminarAsync(Mesa entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await ObtenerPorIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await EliminarAsync(entity, cancellationToken);
            }
        }

        public async Task<(IEnumerable<Mesa> Items, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var total = await _dbSet.CountAsync(cancellationToken);
            var items = await _dbSet
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (Items: items, Total: total);
        }

        public async Task<IEnumerable<Mesa>> BuscarAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return _dbSet.Where(predicado).ToList();
        }

        public Task<bool> ExisteAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Any(predicado));
        }

        public Task<int> ContarAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Count(predicado));
        }

        public Task<Mesa?> PrimeroODefaultAsync(Func<Mesa, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.FirstOrDefault(predicado));
        }

        public async Task<IEnumerable<Mesa>> ObtenerPorSpecAsync(ISpecification<Mesa> specification, CancellationToken cancellationToken = default)
        {
            // Esta es una implementación básica que asume que ISpecification tiene una propiedad Criteria
            // En una implementación real, habría que adaptar esto
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public Task<int> ContarPorSpecAsync(ISpecification<Mesa> specification, CancellationToken cancellationToken = default)
        {
            // Esta es una implementación básica
            return Task.FromResult(0);
        }

        public Task<Mesa?> PrimeroODefaultPorSpecAsync(ISpecification<Mesa> specification, CancellationToken cancellationToken = default)
        {
            // Esta es una implementación básica
            return Task.FromResult<Mesa?>(null);
        }

        public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Estas implementaciones dependen de la firma exacta del método en la interfaz Repository<T>
        Task<Mesa?> IRepository<Mesa>.ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return _dbSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
        }

        Task IRepository<Mesa>.AgregarAsync(Mesa entity, CancellationToken cancellationToken)
        {
            return _dbSet.AddAsync(entity, cancellationToken).AsTask();
        }

        Task IRepository<Mesa>.ActualizarAsync(Mesa entity, CancellationToken cancellationToken)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        #endregion
    }
} 