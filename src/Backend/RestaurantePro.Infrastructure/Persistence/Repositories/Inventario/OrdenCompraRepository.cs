using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Inventario
{
    /// <summary>
    /// Implementación del repositorio de órdenes de compra
    /// </summary>
    public class OrdenCompraRepository : Repository<OrdenCompra>, IOrdenCompraRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public OrdenCompraRepository(RestauranteProDbContext dbContext, ILogger<OrdenCompraRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Obtiene una orden de compra por su identificador
        /// </summary>
        public new async Task<OrdenCompra> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            
            if (entidad == null)
                throw new KeyNotFoundException($"No se encontró la orden de compra con ID {id}");
                
            return entidad;
        }

        /// <summary>
        /// Obtiene todas las órdenes de compra
        /// </summary>
        public async Task<IQueryable<OrdenCompra>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        {
            return _dbSet
                .Include(o => o.Items)
                .OrderByDescending(o => o.FechaEmision);
        }

        /// <summary>
        /// Obtiene órdenes de compra por su estado
        /// </summary>
        public async Task<IEnumerable<OrdenCompra>> ObtenerPorEstadoAsync(EstadoOrdenCompra estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.Estado == estado)
                .OrderByDescending(o => o.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las órdenes de compra de un proveedor específico
        /// </summary>
        public async Task<IEnumerable<OrdenCompra>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.ProveedorId == proveedorId)
                .OrderByDescending(o => o.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las órdenes de compra que contienen un ingrediente específico
        /// </summary>
        public async Task<IEnumerable<OrdenCompra>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.Items.Any(i => i.IngredienteId == ingredienteId))
                .OrderByDescending(o => o.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las órdenes de compra en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<OrdenCompra>> ObtenerPorRangoFechasAsync(DateTime desde, DateTime hasta, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.FechaEmision >= desde && o.FechaEmision <= hasta)
                .OrderByDescending(o => o.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Agrega una nueva orden de compra
        /// </summary>
        public new async Task AgregarAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(ordenCompra, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza una orden de compra existente
        /// </summary>
        public new async Task ActualizarAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default)
        {
            _dbContext.Attach(ordenCompra);
            _dbContext.Entry(ordenCompra).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene órdenes de compra pendientes para un proveedor específico
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerPendientesPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => o.ProveedorId == proveedorId && o.Estado == EstadoOrdenCompra.Pendiente)
                .OrderByDescending(o => o.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una orden de compra con sus items por su identificador
        /// </summary>
        public async Task<OrdenCompra> ObtenerPorIdConItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
                
            if (entidad == null)
                throw new KeyNotFoundException($"No se encontró la orden de compra con ID {id}");
                
            return entidad;
        }

        /// <summary>
        /// Agrega una nueva orden de compra (Alias de AgregarAsync)
        /// </summary>
        public async Task AddAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default)
        {
            await AgregarAsync(ordenCompra, cancellationToken);
        }
    }
} 