using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Clientes;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    /// <summary>
    /// Implementación del repositorio de tarjetas de fidelización
    /// </summary>
    public class TarjetaFidelizacionRepository : Repository<TarjetaFidelizacion>, ITarjetaFidelizacionRepository
    {
        private readonly RestauranteProDbContext _dbContext;
        private readonly ILogger<TarjetaFidelizacionRepository> _logger;

        public TarjetaFidelizacionRepository(RestauranteProDbContext dbContext, ILogger<TarjetaFidelizacionRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su ID
        /// </summary>
        public async Task<TarjetaFidelizacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken, bool asNoTracking)
        {
            var query = _dbContext.TarjetasFidelizacion
                .Where(t => t.Id == id && !t.EstaEliminado);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su ID (retorna null si no existe)
        /// </summary>
        public async Task<TarjetaFidelizacion?> ObtenerPorIdSinExcepcionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su código
        /// </summary>
        public async Task<TarjetaFidelizacion> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken)
        {
            var tarjeta = await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Codigo == codigo, cancellationToken);

            return tarjeta ?? throw new KeyNotFoundException($"No se encontró la tarjeta de fidelización con código {codigo}");
        }

        /// <summary>
        /// Obtiene una tarjeta de fidelización por su número
        /// </summary>
        public async Task<TarjetaFidelizacion?> ObtenerPorNumeroAsync(string numero, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Codigo == numero, cancellationToken);
        }

        /// <summary>
        /// Verifica si existe una tarjeta con el número especificado
        /// </summary>
        public Task<bool> ExisteNumeroTarjetaAsync(string numero, CancellationToken cancellationToken = default)
        {
            return _dbSet.AnyAsync(t => t.Codigo == numero, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las tarjetas de un cliente
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.ClienteId == clienteId)
                .OrderByDescending(t => t.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene la tarjeta activa de un cliente
        /// </summary>
        public async Task<TarjetaFidelizacion> ObtenerTarjetaActivaPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            var tarjeta = await _dbSet
                .FirstOrDefaultAsync(t => t.ClienteId == clienteId && t.Estado == EstadoTarjeta.Activa, cancellationToken);

            return tarjeta ?? throw new KeyNotFoundException($"No se encontró una tarjeta de fidelización activa para el cliente con ID {clienteId}");
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por estado
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorEstadoAsync(EstadoTarjeta estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.Estado == estado)
                .OrderByDescending(t => t.FechaEmision)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por nivel de fidelización
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerPorNivelAsync(NivelFidelizacion nivel, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.NivelFidelizacion == nivel && t.Estado == EstadoTarjeta.Activa)
                .OrderByDescending(t => t.PuntosAcumulados)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las tarjetas filtradas por varios niveles de fidelización
        /// </summary>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerTarjetasPorNivelesAsync(IEnumerable<NivelFidelizacion> niveles, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => niveles.Contains(t.NivelFidelizacion) && t.Estado == EstadoTarjeta.Activa)
                .OrderByDescending(t => t.PuntosAcumulados)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Genera un código único para una nueva tarjeta
        /// </summary>
        public async Task<string> GenerarCodigoUnicoAsync(string prefijo = "TF", CancellationToken cancellationToken = default)
        {
            string codigo;
            do
            {
                codigo = $"{prefijo}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            } while (await ExisteNumeroTarjetaAsync(codigo, cancellationToken));
            return codigo;
        }
        
        /// <summary>
        /// Elimina una tarjeta de fidelización
        /// </summary>
        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tarjeta = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (tarjeta != null)
            {
                tarjeta.Eliminar();
                _dbSet.Update(tarjeta);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TarjetaFidelizacion>> ObtenerConPuntosProximosAExpirarAsync(DateTime fechaExpiracion, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Obteniendo tarjetas de fidelización con puntos a expirar antes de: {FechaExpiracion}", fechaExpiracion);
            
            return await _dbSet
                .AsNoTracking()
                .Where(t => 
                    t.Estado == EstadoTarjeta.Activa &&
                    t.FechaExpiracion != null &&
                    t.FechaExpiracion <= fechaExpiracion
                )
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene el historial de puntos de una tarjeta
        /// </summary>
        public async Task<List<HistorialPuntos>> ObtenerHistorialPuntosAsync(
            Guid tarjetaFidelizacionId,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var skip = (pageNumber - 1) * pageSize;
            return await _dbContext.Set<HistorialPuntos>()
                .Where(h => h.TarjetaFidelizacionId == tarjetaFidelizacionId)
                .OrderByDescending(h => h.FechaOperacion)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las estadísticas de una tarjeta
        /// </summary>
        public async Task<EstadisticasTarjeta> ObtenerEstadisticasAsync(
            Guid tarjetaFidelizacionId,
            CancellationToken cancellationToken = default)
        {
            var tarjeta = await _dbSet
                .Include(t => t.HistorialPuntos)
                .FirstOrDefaultAsync(t => t.Id == tarjetaFidelizacionId, cancellationToken);

            if (tarjeta == null)
                throw new KeyNotFoundException($"No se encontró la tarjeta de fidelización con ID {tarjetaFidelizacionId}");

            var historial = await _dbContext.Set<HistorialPuntos>()
                .Where(h => h.TarjetaFidelizacionId == tarjetaFidelizacionId)
                .ToListAsync(cancellationToken);

            var estadisticas = new EstadisticasTarjeta
            {
                PuntosAcumulados = historial.Where(h => h.Puntos > 0).Sum(h => h.Puntos),
                PuntosCanjeados = historial.Where(h => h.Puntos < 0).Sum(h => Math.Abs(h.Puntos)),
                TotalMovimientos = historial.Count,
                MontoTotalGastado = historial.Sum(h => h.MontoCompra ?? 0),
                UltimaActividad = historial.OrderByDescending(h => h.FechaOperacion).FirstOrDefault()?.FechaOperacion
            };

            return estadisticas;
        }

        /// <summary>
        /// Actualiza una tarjeta de fidelización con estrategia específica para SQLite
        /// </summary>
        public new async Task ActualizarAsync(TarjetaFidelizacion entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Iniciando actualización segura de tarjeta de fidelización {TarjetaId}", entity.Id);

                // Estrategia 1: Detección forzada de cambios
                _dbContext.ChangeTracker.DetectChanges();

                // Estrategia 2: Obtener la entidad existente (trackeada) o adjuntar si no está
                var entry = _dbContext.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    _logger.LogInformation("Tarjeta {TarjetaId} no está trackeada, adjuntando...", entity.Id);
                    _dbSet.Attach(entity);
                    entry = _dbContext.Entry(entity);
                }

                // Estrategia 3: Marcar explícitamente como modificada
                entry.State = EntityState.Modified;

                // Estrategia 4: Marcar propiedades específicas como modificadas
                entry.Property(e => e.PuntosDisponibles).IsModified = true;
                entry.Property(e => e.PuntosAcumulados).IsModified = true;
                entry.Property(e => e.Estado).IsModified = true;
                entry.Property(e => e.NivelFidelizacion).IsModified = true;
                entry.Property(e => e.FechaActualizacion).IsModified = true;

                // Estrategia 5: Guardar cambios con manejo de concurrencia
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Tarjeta de fidelización {TarjetaId} actualizada exitosamente", entity.Id);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning("Concurrencia detectada en tarjeta {TarjetaId}, aplicando estrategia de recuperación", entity.Id);
                    
                    // Estrategia 6: Recuperación de concurrencia
                    await ManejarConcurrenciaTarjeta(entity, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la tarjeta de fidelización {TarjetaId}", entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Maneja la concurrencia en tarjetas de fidelización
        /// </summary>
        private async Task ManejarConcurrenciaTarjeta(TarjetaFidelizacion entity, CancellationToken cancellationToken)
        {
            try
            {
                // Estrategia 1: Detach y reattach
                _dbContext.Entry(entity).State = EntityState.Detached;
                
                // Estrategia 2: Recargar desde BD
                var entidadRecargada = await _dbSet
                    .Include(t => t.HistorialPuntos)
                    .FirstOrDefaultAsync(t => t.Id == entity.Id, cancellationToken);

                if (entidadRecargada == null)
                {
                    throw new InvalidOperationException($"No se encontró la tarjeta de fidelización con Id {entity.Id} después de concurrencia");
                }

                // Estrategia 3: Aplicar cambios sobre la entidad recargada
                entidadRecargada.ActualizarPuntos(entity.PuntosAcumulados, entity.PuntosDisponibles);
                entidadRecargada.ActualizarNivelInterno(entity.NivelFidelizacion);
                entidadRecargada.ActualizarEstado(entity.Estado);

                // Estrategia 4: Guardar cambios
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Tarjeta de fidelización {TarjetaId} actualizada exitosamente después de concurrencia", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en manejo de concurrencia para tarjeta {TarjetaId}", entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Marca una tarjeta como modificada con estrategias de persistencia forzada para SQLite
        /// </summary>
        public new void Update(TarjetaFidelizacion entity)
        {
            _logger.LogInformation("Aplicando estrategias de persistencia forzada para tarjeta {TarjetaId}", entity.Id);
            
            try
            {
                // Estrategia 1: Detección forzada de cambios
                _dbContext.ChangeTracker.DetectChanges();
                
                // Estrategia 2: Marcado explícito como modificado
                _dbSet.Update(entity);
                
                // Estrategia 3: Marcado específico de propiedades críticas
                _dbContext.Entry(entity).Property(e => e.PuntosDisponibles).IsModified = true;
                _dbContext.Entry(entity).Property(e => e.PuntosAcumulados).IsModified = true;
                _dbContext.Entry(entity).Property(e => e.Estado).IsModified = true;
                _dbContext.Entry(entity).Property(e => e.NivelFidelizacion).IsModified = true;
                
                _logger.LogInformation("Estrategias de persistencia aplicadas correctamente para tarjeta {TarjetaId}", entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aplicar estrategias de persistencia para tarjeta {TarjetaId}", entity.Id);
                throw;
            }
        }

        /// <summary>
        /// Obtiene los puntos acumulados en el mes actual para una tarjeta
        /// </summary>
        public async Task<int> ObtenerPuntosAcumuladosMesActualAsync(
            Guid tarjetaFidelizacionId,
            CancellationToken cancellationToken = default)
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var puntosMes = await _dbContext.Set<HistorialPuntos>()
                .Where(h => h.TarjetaFidelizacionId == tarjetaFidelizacionId &&
                           h.FechaOperacion >= inicioMes &&
                           h.FechaOperacion <= finMes &&
                           h.Puntos > 0) // Solo puntos agregados, no canjeados
                .SumAsync(h => h.Puntos, cancellationToken);

            return puntosMes;
        }
    }
} 