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
        private readonly ILogger<OrdenCompraRepository> _logger;

        public OrdenCompraRepository(RestauranteProDbContext dbContext, ILogger<OrdenCompraRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una orden de compra por su identificador
        /// </summary>
        public new async Task<OrdenCompra> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

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
        public override async Task ActualizarAsync(OrdenCompra ordenCompra, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🔧 [OrdenCompraRepository] ⚡ OVERRIDE ActualizarAsync EJECUTÁNDOSE para orden {OrdenId} con estado {Estado}", 
                ordenCompra.Id, ordenCompra.Estado);
            
            try
            {
                // FORZAR la detección de cambios antes de cualquier operación
                _dbContext.ChangeTracker.DetectChanges();
                
                // Verificar el estado de rastreo de la entidad
                var entry = _dbContext.Entry(ordenCompra);
                _logger.LogInformation("🔧 [OrdenCompraRepository] 📊 Estado de rastreo inicial: {EstadoRastreo}", entry.State);
                
                // Estrategia 1: Si está desconectada, conectarla y actualizar
                if (entry.State == EntityState.Detached)
                {
                    _logger.LogInformation("🔧 [OrdenCompraRepository] 🔄 Entidad desconectada, conectando...");
                    _dbContext.Attach(ordenCompra);
                    entry = _dbContext.Entry(ordenCompra);
                }
                
                // Estrategia 2: Marcar explícitamente la propiedad Estado como modificada
                _logger.LogInformation("🔧 [OrdenCompraRepository] 🔄 Marcando Estado como modificado...");
                entry.Property(nameof(OrdenCompra.Estado)).IsModified = true;
                
                // Estrategia 3: Verificar que el cambio se detectó
                var estadoProperty = entry.Property(nameof(OrdenCompra.Estado));
                _logger.LogInformation("🔧 [OrdenCompraRepository] 📊 Propiedad Estado - IsModified: {IsModified}, Valor: {Valor}", 
                    estadoProperty.IsModified, estadoProperty.CurrentValue);
                
                // Estrategia 4: Forzar el estado de la entidad a Modified si es necesario
                if (entry.State != EntityState.Modified)
                {
                    _logger.LogInformation("🔧 [OrdenCompraRepository] 🔄 Forzando estado de entidad a Modified...");
                    entry.State = EntityState.Modified;
                }
                
                _logger.LogInformation("🔧 [OrdenCompraRepository] 💾 Guardando cambios en BD...");
                var changesSaved = await _dbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("🔧 [OrdenCompraRepository] ✅ Cambios guardados: {CambiosGuardados}, Estado final: {EstadoFinal}", 
                    changesSaved, ordenCompra.Estado);
                
                // Verificar que el cambio se persistió
                var entidadVerificacion = await _dbContext.OrdenesCompra.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == ordenCompra.Id, cancellationToken);
                
                if (entidadVerificacion != null)
                {
                    _logger.LogInformation("🔧 [OrdenCompraRepository] 🔍 Estado en BD después de guardar: {EstadoBD}", 
                        entidadVerificacion.Estado);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔧 [OrdenCompraRepository] ❌ Error al actualizar orden de compra {OrdenId}", ordenCompra.Id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una orden de compra por su identificador incluyendo los items
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
        /// Obtiene órdenes de compra pendientes para un proveedor específico
        /// </summary>
        public async Task<List<OrdenCompra>> ObtenerPendientesPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Where(o => o.ProveedorId == proveedorId && o.Estado == EstadoOrdenCompra.Pendiente)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una orden de compra por ID sin rastreo para forzar lectura desde BD
        /// </summary>
        public async Task<OrdenCompra?> ObtenerPorIdSinRastreoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🔍 [OrdenCompraRepository] Obteniendo orden sin rastreo desde BD: {OrdenId}", id);
            
            var orden = await _dbSet
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
                
            if (orden != null)
            {
                _logger.LogInformation("✅ [OrdenCompraRepository] Orden obtenida desde BD con estado: {Estado}", orden.Estado);
            }
            else
            {
                _logger.LogWarning("⚠️ [OrdenCompraRepository] Orden no encontrada en BD: {OrdenId}", id);
            }
            
            return orden;
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