using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(DbContext dbContext, ILogger<ProductoRepository> logger) : base(dbContext, logger)
        {
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
            }
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.CategoriaId == categoriaId);
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
            }
            return await query.ToListAsync(cancellationToken);
        }

        public Task<IEnumerable<Producto>> ObtenerProductosPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            // Esta lógica necesita ser revisada ya que la entidad Producto no tiene una colección de Recetas directamente.
            // Devolver una lista vacía temporalmente para permitir que el proyecto compile.
            return Task.FromResult(Enumerable.Empty<Producto>());
        }

        public async Task<bool> ExisteProductoPorNombre(string nombre, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();
            return await query.AnyAsync(p => p.Nombre == nombre, cancellationToken);
        }

        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await base.EliminarPorIdAsync(id, cancellationToken);
        }
    }
} 