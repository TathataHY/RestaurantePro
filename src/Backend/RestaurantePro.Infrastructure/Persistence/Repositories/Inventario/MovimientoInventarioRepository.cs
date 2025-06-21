using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
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
    /// Implementación del repositorio de movimientos de inventario
    /// </summary>
    public class MovimientoInventarioRepository : Repository<MovimientoInventario>, IMovimientoInventarioRepository
    {
        private new readonly RestauranteProDbContext _dbContext;

        public MovimientoInventarioRepository(RestauranteProDbContext dbContext, ILogger<MovimientoInventarioRepository> logger)
            : base(dbContext, logger)
        {
            _dbContext = dbContext;
        }

        // TODO: Este método es incorrecto para un tipo de entidad poseída.
        // Los movimientos deben obtenerse a través de su Ingrediente propietario.
        // Comentado para evitar su uso. Se necesita un rediseño si se requiere esta funcionalidad.
        /*
        /// <summary>
        /// Obtiene un movimiento por su ID
        /// </summary>
        public new async Task<MovimientoInventario> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entidad = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            
            if (entidad == null)
                throw new KeyNotFoundException($"No se encontró el movimiento de inventario con ID {id}");
                
            return entidad;
        }
        */

        /// <summary>
        /// Obtiene todos los movimientos de un ingrediente
        /// </summary>
        public async Task<IEnumerable<MovimientoInventario>> ObtenerPorIngredienteAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            var ingrediente = await _dbContext.Ingredientes
                                          .Include(i => i.Movimientos)
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(i => i.Id == ingredienteId, cancellationToken);

            if (ingrediente == null)
            {
                return Enumerable.Empty<MovimientoInventario>();
            }

            return ingrediente.Movimientos.OrderByDescending(m => m.Fecha);
        }

        // TODO: Este método es incorrecto para un tipo de entidad poseída (owned entity type).
        // No se puede consultar una entidad poseída directamente a nivel global.
        // Se debe consultar a través de los Ingredientes. Comentado para evitar su uso.
        /*
        /// <summary>
        /// Obtiene los movimientos de inventario en un rango de fechas
        /// </summary>
        public async Task<IEnumerable<MovimientoInventario>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene los movimientos por tipo
        /// </summary>
        public async Task<IEnumerable<MovimientoInventario>> ObtenerPorTipoAsync(TipoMovimientoInventario tipoMovimiento, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.TipoMovimiento == tipoMovimiento)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync(cancellationToken);
        }
        */

        // TODO: Incorrecto para un tipo poseído. El registro debe hacerse en el agregado raíz (Ingrediente).
        /*
        /// <summary>
        /// Registra un nuevo movimiento de inventario
        /// </summary>
        public async Task<MovimientoInventario> RegistrarAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(movimiento, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return movimiento;
        }
        */

        // TODO: Incorrecto para un tipo poseído. La actualización debe hacerse en el agregado raíz (Ingrediente).
        /*
        /// <summary>
        /// Actualiza un movimiento existente
        /// </summary>
        public new async Task ActualizarAsync(MovimientoInventario movimiento, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(movimiento).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        */

        /// <summary>
        /// Calcula el stock actual de un ingrediente basado en sus movimientos
        /// </summary>
        public async Task<decimal> CalcularStockActualAsync(Guid ingredienteId)
        {
            var movimientos = await _dbContext.Ingredientes
                .Where(i => i.Id == ingredienteId)
                .SelectMany(i => i.Movimientos.Where(m => m.EstaAplicado))
                .ToListAsync();

            if (!movimientos.Any())
            {
                return 0;
            }

            decimal stock = 0;

            foreach (var movimiento in movimientos.OrderBy(m => m.Fecha))
            {
                if (movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso)
                {
                    stock += movimiento.Cantidad;
                }
                else
                {
                    stock -= movimiento.Cantidad;
                }
            }

            return stock;
        }

        /// <summary>
        /// Agrega un nuevo movimiento
        /// </summary>
        public async Task AgregarAsync(MovimientoInventario movimiento)
        {
            // Esta implementación es inherentemente incorrecta para un tipo poseído.
            // La entidad poseída debe ser agregada a su propietario (Ingrediente)
            // y luego el propietario debe ser guardado.
            // Se necesita refactorizar el código que llama a este método.
            var ingrediente = await _dbContext.Ingredientes
                                          .Include(i => i.Movimientos)
                                          .FirstOrDefaultAsync(i => i.Id == movimiento.IngredienteId);

            if (ingrediente != null)
            {
                ingrediente.AgregarMovimiento(movimiento);
            }
            else
            {
                // Lanzar una excepción o manejar el caso donde el ingrediente no existe
                throw new InvalidOperationException($"No se puede agregar movimiento a un ingrediente inexistente: {movimiento.IngredienteId}");
            }
        }

        /// <summary>
        /// Guarda los cambios en la base de datos
        /// </summary>
        public async Task GuardarCambiosAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
} 