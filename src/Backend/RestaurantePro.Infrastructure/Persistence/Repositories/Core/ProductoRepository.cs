using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Core.Productos.Repositories;
using RestaurantePro.Infrastructure.Persistence.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Core
{
    /// <summary>
    /// Repositorio para la entidad Producto
    /// </summary>
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(DbContext dbContext, ILogger<ProductoRepository> logger)
            : base(dbContext, logger)
        {
        }

        public async Task<IEnumerable<Producto>> GetProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Productos
                .Where(p => p.CategoriaId == categoriaId && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string termino, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Productos
                .Where(p => p.Nombre.Contains(termino) || p.Descripcion.Contains(termino))
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<Producto> GetProductoConDetallesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Ingredientes)
                    .ThenInclude(i => i.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> ExisteProductoConNombreAsync(string nombre, Guid? exceptoId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Productos.AsQueryable();
            
            if (exceptoId.HasValue)
                query = query.Where(p => p.Id != exceptoId.Value);
            
            return await query.AnyAsync(p => p.Nombre == nombre, cancellationToken);
        }

        /// <summary>
        /// Obtiene los productos activos
        /// </summary>
        /// <returns>Lista de productos activos</returns>
        public async Task<IEnumerable<Producto>> ObtenerProductosActivosAsync()
        {
            return await _dbSet
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        
        /// <summary>
        /// Obtiene los productos por categoría
        /// </summary>
        /// <param name="categoria">Categoría de productos</param>
        /// <returns>Lista de productos de la categoría</returns>
        public async Task<IEnumerable<Producto>> ObtenerProductosPorCategoriaAsync(string categoria)
        {
            return await _dbSet
                .Where(p => p.Activo && p.Categoria == categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        
        /// <summary>
        /// Obtiene los productos más vendidos
        /// </summary>
        /// <param name="cantidad">Cantidad de productos a obtener</param>
        /// <returns>Lista de productos más vendidos</returns>
        public async Task<IEnumerable<Producto>> ObtenerProductosMasVendidosAsync(int cantidad = 10)
        {
            // En una implementación real, aquí se haría un JOIN con la tabla de ventas
            // para obtener los productos más vendidos.
            // Por ahora, simplemente devolvemos los productos ordenados por precio.
            return await _dbSet
                .Where(p => p.Activo)
                .OrderByDescending(p => p.Precio)
                .Take(cantidad)
                .ToListAsync();
        }
        
        /// <summary>
        /// Busca productos por nombre o descripción
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <returns>Lista de productos que coinciden con el término</returns>
        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerProductosActivosAsync();
            
            var terminoLower = termino.ToLower();
            
            return await _dbSet
                .Where(p => p.Activo && 
                       (p.Nombre.ToLower().Contains(terminoLower) || 
                        p.Descripcion.ToLower().Contains(terminoLower)))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
} 