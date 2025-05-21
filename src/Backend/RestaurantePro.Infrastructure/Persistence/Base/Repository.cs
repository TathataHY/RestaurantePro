using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Base
{
    /// <summary>
    /// Implementación base para todos los repositorios que utilizan Entity Framework Core
    /// </summary>
    /// <typeparam name="T">Tipo de entidad que maneja el repositorio</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly RestauranteProDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(RestauranteProDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <inheritdoc />
        public virtual async Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task AgregarAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task AgregarRangoAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task ActualizarAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task EliminarAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual async Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await ObtenerPorIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await EliminarAsync(entity, cancellationToken);
            }
        }

        /// <inheritdoc />
        public virtual async Task<(IEnumerable<T> Items, int Total)> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var totalCount = await _dbSet.CountAsync(cancellationToken);
            var items = await _dbSet
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<T>> BuscarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Nota: Idealmente deberíamos usar un proveedor de LINQ asíncrono, pero por ahora
            // usamos la implementación más simple
            return _dbSet.Where(predicado).ToList();
        }

        /// <inheritdoc />
        public virtual Task<bool> ExisteAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Nota: Idealmente deberíamos usar un proveedor de LINQ asíncrono
            return Task.FromResult(_dbSet.Any(predicado));
        }

        /// <inheritdoc />
        public virtual Task<int> ContarAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Nota: Idealmente deberíamos usar un proveedor de LINQ asíncrono
            return Task.FromResult(_dbSet.Count(predicado));
        }

        /// <inheritdoc />
        public virtual Task<T?> PrimeroODefaultAsync(Func<T, bool> predicado, CancellationToken cancellationToken = default)
        {
            // Nota: Idealmente deberíamos usar un proveedor de LINQ asíncrono
            return Task.FromResult(_dbSet.FirstOrDefault(predicado));
        }

        /// <inheritdoc />
        public virtual async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 