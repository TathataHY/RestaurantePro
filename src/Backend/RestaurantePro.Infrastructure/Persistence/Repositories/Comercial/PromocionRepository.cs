using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Comercial
{
    /// <summary>
    /// Repositorio para la entidad Promocion
    /// </summary>
    public class PromocionRepository : Repository<Promocion>, IPromocionRepository
    {
        private readonly IDateTimeService _dateTimeService;

        public PromocionRepository(
            DbContext dbContext, 
            ILogger<PromocionRepository> logger,
            IDateTimeService dateTimeService)
            : base(dbContext, logger)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Obtiene las promociones activas en una fecha determinada
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesActivasAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Estado == EstadoPromocion.Activa 
                           && p.FechaInicio <= fecha 
                           && p.FechaFin >= fecha)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las promociones activas y vigentes en la fecha actual
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesActivasAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            return await ObtenerPromocionesActivasAsync(fechaActual, cancellationToken);
        }

        /// <summary>
        /// Obtiene las promociones por estado
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesPorEstadoAsync(EstadoPromocion estado, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Estado == estado)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las promociones aplicables a un producto específico
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesPorProductoAsync(Guid productoId, Guid? categoriaId = null, CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            
            var query = _dbSet
                .Where(p => p.Estado == EstadoPromocion.Activa 
                           && p.FechaInicio <= fechaActual 
                           && p.FechaFin >= fechaActual);

            // Si no se especifica producto ni categoría, devolver todas las promociones activas
            if (productoId == Guid.Empty && !categoriaId.HasValue)
            {
                return await query.ToListAsync(cancellationToken);
            }

            // Filtrar por producto o categoría
            return await query
                .Where(p => p.ProductosAplicablesIds.Contains(productoId) ||
                           (categoriaId.HasValue && p.CategoriasAplicablesIds.Contains(categoriaId.Value)) ||
                           (p.ProductosAplicablesIds.Count == 0 && p.CategoriasAplicablesIds.Count == 0))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene una promoción por su código
        /// </summary>
        public async Task<Promocion?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);
        }

        /// <summary>
        /// Obtiene las promociones que un cliente ha utilizado
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesUsadasPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.ClientesQueUsaronIds.Contains(clienteId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene las promociones que están próximas a vencer
        /// </summary>
        public async Task<IEnumerable<Promocion>> ObtenerPromocionesProximasAVencerAsync(int diasLimite, CancellationToken cancellationToken = default)
        {
            var fechaLimite = _dateTimeService.Now.AddDays(diasLimite);
            
            return await _dbSet
                .Where(p => p.Estado == EstadoPromocion.Activa 
                           && p.FechaFin <= fechaLimite 
                           && p.FechaFin >= _dateTimeService.Now)
                .OrderBy(p => p.FechaFin)
                .ToListAsync(cancellationToken);
        }
    }
} 