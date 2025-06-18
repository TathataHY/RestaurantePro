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
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        private readonly RestauranteProDbContext _restauranteProDbContext;

        public ProductoRepository(RestauranteProDbContext dbContext, ILogger<ProductoRepository> logger) : base(dbContext, logger)
        {
            _restauranteProDbContext = dbContext;
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

        public async Task<List<Producto>> ObtenerProductosPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            return await _restauranteProDbContext.Productos
                .Include(p => p.Recetas)
                .ThenInclude(r => r.Ingredientes)
                .Where(p => p.Recetas.Any(r => r.Ingredientes.Any(i => i.IngredienteId == ingredienteId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteProductoPorNombre(string nombre, CancellationToken cancellationToken = default)
        {
            return await _restauranteProDbContext.Productos.AnyAsync(p => p.Nombre == nombre && !p.EstaEliminado, cancellationToken);
        }

        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var producto = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (producto != null)
            {
                // No lo eliminamos, solo lo marcamos para que el interceptor actúe
                _dbSet.Remove(producto);
            }
        }
    }
} 