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
using System.Linq.Expressions;

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
            _logger.LogInformation("🔍 ProductoRepository.ObtenerTodosAsync - soloActivos: {SoloActivos}", soloActivos);
            
            var query = _dbSet.AsNoTracking().AsQueryable();
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
                _logger.LogInformation("🔍 Aplicando filtro: Solo productos activos");
            }
            else
            {
                query = query.Where(p => !p.EstaActivo);
                _logger.LogInformation("🔍 Aplicando filtro: Solo productos inactivos");
            }
            
            var productos = await query.ToListAsync(cancellationToken);
            _logger.LogInformation("🔍 Productos obtenidos: {Count} productos", productos.Count);
            
            return productos;
        }

        public async Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(p => p.CategoriaId == categoriaId);
            if (soloActivos)
            {
                query = query.Where(p => p.EstaActivo);
            }
            // Si soloActivos = false, no aplicar filtro adicional (obtener todos los productos)
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Producto>> ObtenerProductosPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            return await _restauranteProDbContext.Productos.AsNoTracking()
                .Include(p => p.Recetas)
                .ThenInclude(r => r.Ingredientes)
                .Where(p => p.Recetas.Any(r => r.Ingredientes.Any(i => i.IngredienteId == ingredienteId)))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteProductoPorNombre(string nombre, CancellationToken cancellationToken = default)
        {
            return await _restauranteProDbContext.Productos.AnyAsync(p => p.Nombre == nombre && !p.EstaEliminado, cancellationToken);
        }

        public async Task<Producto?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken = default)
        {
            return await _restauranteProDbContext.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Nombre == nombre && !p.EstaEliminado, cancellationToken);
        }

        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var producto = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (producto != null)
            {
                producto.MarkAsDeleted();
                _dbSet.Update(producto);
            }
        }

        public async Task<List<Producto>> BuscarAsync(Expression<Func<Producto, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _restauranteProDbContext.Productos.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<List<Producto>> ObtenerProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken)
        {
            return await ObtenerPorCategoriaAsync(categoriaId, true, cancellationToken);
        }

        public async Task<bool> ExisteProductoConNombreAsync(string nombre, CancellationToken cancellationToken)
        {
            return await ExisteProductoPorNombre(nombre, cancellationToken);
        }

        /// <summary>
        /// Elimina físicamente un producto (para tests)
        /// </summary>
        public async Task EliminarFisicamenteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var producto = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (producto != null)
            {
                _dbSet.Remove(producto); // Eliminación física real
            }
        }
    }
} 