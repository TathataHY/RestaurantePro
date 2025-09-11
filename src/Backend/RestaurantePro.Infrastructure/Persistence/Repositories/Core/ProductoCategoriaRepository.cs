using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    public class ProductoCategoriaRepository : Repository<ProductoCategoria>, IProductoCategoriaRepository
    {
        public ProductoCategoriaRepository(DbContext dbContext, ILogger<ProductoCategoriaRepository> logger) : base(dbContext, logger)
        {
        }

        public async Task<ProductoCategoria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(c => c.Id == id && c.EstaActivo).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ProductoCategoria?> ObtenerPorIdIncluyendoInactivasAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await base.ObtenerPorIdAsync(id, cancellationToken);
        }

        public async Task<List<ProductoCategoria>> ObtenerActivasAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(c => c.EstaActivo).ToListAsync(cancellationToken);
        }

        public async Task<List<ProductoCategoria>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task AgregarAsync(ProductoCategoria categoria, CancellationToken cancellationToken = default)
        {
            await base.AgregarAsync(categoria, cancellationToken);
        }

        public async Task ActualizarAsync(ProductoCategoria categoria, CancellationToken cancellationToken = default)
        {
            await base.ActualizarAsync(categoria, cancellationToken);
        }

        public async Task<bool> ExisteConNombreAsync(string nombre, Guid? idExcluir = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(c => c.Nombre == nombre);
            
            if (idExcluir.HasValue)
            {
                query = query.Where(c => c.Id != idExcluir.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }
    }
} 