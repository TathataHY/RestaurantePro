using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.Interfaces;
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

        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        public override async Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los productos disponibles
        /// </summary>
        public async Task<IEnumerable<Producto>> ObtenerTodosAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (soloActivos)
                query = query.Where(p => p.Activo);
                
            return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene productos por categoría
        /// </summary>
        public async Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, bool soloActivos = true, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.CategoriaId == categoriaId);
            
            if (soloActivos)
                query = query.Where(p => p.Activo);
                
            return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene productos que utilizan un ingrediente específico
        /// </summary>
        public async Task<IEnumerable<Producto>> ObtenerProductosPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Producto>()
                .Include(p => p.Ingredientes)
                .Where(p => p.Ingredientes.Any(i => i.IngredienteId == ingredienteId))
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega un nuevo producto
        /// </summary>
        public Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default)
        {
            _dbSet.Add(producto);
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public Task ActualizarAsync(Producto producto, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(producto).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Elimina un producto por su ID
        /// </summary>
        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var producto = await ObtenerPorIdAsync(id, cancellationToken);
            if (producto != null)
            {
                producto.Activo = false;
                await ActualizarAsync(producto, cancellationToken);
            }
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

        public async Task<IEnumerable<Producto>> GetProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Producto>()
                .Where(p => p.CategoriaId == categoriaId && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Producto>> BuscarProductosAsync(string termino, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync(true, cancellationToken);
            
            var terminoLower = termino.ToLower();
            
            return await _dbSet
                .Where(p => p.Activo && 
                       (p.Nombre.ToLower().Contains(terminoLower) || 
                        p.Descripcion.ToLower().Contains(terminoLower)))
                .OrderBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<Producto> GetProductoConDetallesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<Producto>()
                .Include(p => p.Categoria)
                .Include(p => p.Ingredientes)
                    .ThenInclude(i => i.Ingrediente)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<bool> ExisteProductoConNombreAsync(string nombre, Guid? exceptoId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            
            if (exceptoId.HasValue)
                query = query.Where(p => p.Id != exceptoId.Value);
            
            return await query.AnyAsync(p => p.Nombre == nombre, cancellationToken);
        }
    }
} 