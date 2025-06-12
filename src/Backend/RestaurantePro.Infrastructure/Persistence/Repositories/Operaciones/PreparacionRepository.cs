using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Implementación del repositorio de preparaciones diarias
    /// </summary>
    public class PreparacionRepository : Repository<PreparacionDiaria>, IPreparacionRepository
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly RestauranteProDbContext _dbContext;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PreparacionRepository(
            RestauranteProDbContext context,
            IDateTimeService dateTimeService,
            ILogger<PreparacionRepository> logger) : base(context, logger)
        {
            _dbContext = context;
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Obtiene todas las preparaciones del día actual
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now.Date;
            
            return await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaCreacion.Date == fechaActual)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones disponibles para un producto específico
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDisponiblesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.ProductoId == productoId && 
                            (p.Estado == EstadoPreparacion.Disponible || 
                             p.Estado == EstadoPreparacion.PorVencer) &&
                            p.CantidadDisponible > 0)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las preparaciones por su estado
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorEstadoAsync(EstadoPreparacion estado, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.Estado == estado)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones que vencen en un rango de tiempo específico
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorVencerAsync(int horasAnticipacion = 2, CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            var fechaLimite = fechaActual.AddHours(horasAnticipacion);
            
            return await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaVencimiento.HasValue && 
                            p.FechaVencimiento.Value > fechaActual && 
                            p.FechaVencimiento.Value <= fechaLimite &&
                            p.Estado == EstadoPreparacion.Disponible)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene preparaciones por chef
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorChefAsync(Guid chefId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.ChefId == chefId)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene estadísticas de preparaciones del día actual
        /// </summary>
        public async Task<EstadisticasPreparaciones> ObtenerEstadisticasDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            var preparaciones = await _dbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaCreacion.Date == fechaActual.Date)
                .ToListAsync(cancellationToken);
            
            // Identificar las preparaciones por vencer primero (para contarlas aparte)
            var preparacionesPorVencer = preparaciones
                .Where(p => p.Estado == EstadoPreparacion.Disponible && 
                           p.FechaVencimiento.HasValue &&
                           p.FechaVencimiento.Value <= fechaActual.AddHours(5) && 
                           p.FechaVencimiento.Value > fechaActual)
                .ToList();
                
            // Calcular estadísticas
            var resultado = new EstadisticasPreparaciones
            {
                TotalPreparaciones = preparaciones.Count,
                
                // Contar preparaciones disponibles excluyendo las por vencer
                PreparacionesDisponibles = preparaciones
                    .Count(p => p.Estado == EstadoPreparacion.Disponible && 
                            !preparacionesPorVencer.Any(pv => pv.Id == p.Id)),
                            
                PreparacionesPorVencer = preparacionesPorVencer.Count,
                
                PreparacionesAgotadas = preparaciones
                    .Count(p => p.Estado == EstadoPreparacion.Agotada),
                    
                PreparacionesVencidas = preparaciones
                    .Count(p => p.Estado == EstadoPreparacion.Vencida),
                
                CantidadTotalPreparada = preparaciones.Sum(p => p.CantidadPreparada),
                
                CantidadTotalConsumida = preparaciones.Sum(p => p.CantidadPreparada - p.CantidadDisponible),
                
                CantidadDesperdiciada = preparaciones
                    .Where(p => p.Estado == EstadoPreparacion.Vencida)
                    .Sum(p => p.CantidadDisponible)
            };
            
            // Calcular el porcentaje de eficiencia
            if (resultado.CantidadTotalPreparada > 0)
            {
                resultado.PorcentajeEficiencia = (decimal)resultado.CantidadTotalConsumida / resultado.CantidadTotalPreparada * 100;
            }
            
            return resultado;
        }

        // Implementación de los métodos de IRepository<PreparacionDiaria>

        public override Task<PreparacionDiaria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbSet.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public override async Task<IEnumerable<PreparacionDiaria>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public override Task<PreparacionDiaria> AgregarAsync(PreparacionDiaria entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Add(entity);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entity);
        }

        public override Task<IEnumerable<PreparacionDiaria>> AgregarRangoAsync(IEnumerable<PreparacionDiaria> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.AddRange(entities);
            _dbContext.SaveChangesAsync(cancellationToken);
            return Task.FromResult(entities);
        }

        public override Task ActualizarAsync(PreparacionDiaria entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public override Task EliminarAsync(PreparacionDiaria entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public override async Task EliminarPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await ObtenerPorIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await EliminarAsync(entity, cancellationToken);
            }
        }

        public override async Task<(IEnumerable<PreparacionDiaria> Items, int Total)> ObtenerPaginadoAsync(
            int pagina, int elementosPorPagina, CancellationToken cancellationToken = default)
        {
            var total = await _dbSet.CountAsync(cancellationToken);
            var items = await _dbSet
                .Skip(pagina * elementosPorPagina)
                .Take(elementosPorPagina)
                .ToListAsync(cancellationToken);
            
            return (items, total);
        }

        public Task<IEnumerable<PreparacionDiaria>> BuscarAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Where(predicado).ToList();
            return Task.FromResult<IEnumerable<PreparacionDiaria>>(result);
        }

        public Task<bool> ExisteAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Any(predicado);
            return Task.FromResult(result);
        }

        public Task<int> ContarAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.Count(predicado);
            return Task.FromResult(result);
        }

        public Task<PreparacionDiaria?> PrimeroODefaultAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            var result = _dbSet.FirstOrDefault(predicado);
            return Task.FromResult(result);
        }

        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).ToListAsync(cancellationToken);
        }

        public async Task<int> ContarPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).CountAsync(cancellationToken);
        }

        public async Task<PreparacionDiaria?> PrimeroODefaultPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
        }

        public override Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<PreparacionDiaria> ApplySpecification(ISpecification<PreparacionDiaria> spec)
        {
            return SpecificationEvaluator<PreparacionDiaria>.GetQuery(_dbSet.AsQueryable(), spec);
        }
    }
} 