using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Specifications;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Implementación del repositorio de preparaciones diarias
    /// </summary>
    public class PreparacionRepository : Repository<PreparacionDiaria>, IPreparacionRepository
    {
        private readonly IDateTimeService _dateTimeService;
        private new readonly RestauranteProDbContext _dbContext;
        
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
                .Where(p => p.FechaVencimiento > fechaActual && 
                            p.FechaVencimiento <= fechaLimite &&
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
                           p.FechaVencimiento <= fechaActual.AddHours(5) && 
                           p.FechaVencimiento > fechaActual)
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

        public override async Task<PreparacionDiaria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Id == id && !p.EstaEliminado, cancellationToken);
        }

        public override async Task<IEnumerable<PreparacionDiaria>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public override async Task<PreparacionDiaria> AgregarAsync(PreparacionDiaria entity, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AddAsync(entity, cancellationToken);
            return result.Entity;
        }

        public override async Task<IEnumerable<PreparacionDiaria>> AgregarRangoAsync(IEnumerable<PreparacionDiaria> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            return entities;
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

        public new Task<IEnumerable<PreparacionDiaria>> BuscarAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Where(predicado));
        }

        public new Task<bool> ExisteAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Any(predicado));
        }

        public new Task<int> ContarAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.Count(predicado));
        }

        public new Task<PreparacionDiaria?> PrimeroODefaultAsync(
            Func<PreparacionDiaria, bool> predicado, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dbSet.FirstOrDefault(predicado));
        }

        public new async Task<IEnumerable<PreparacionDiaria>> ObtenerPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.ToListAsync(cancellationToken);
        }

        public new async Task<int> ContarPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.CountAsync(cancellationToken);
        }

        public new async Task<PreparacionDiaria?> PrimeroODefaultPorSpecAsync(
            ISpecification<PreparacionDiaria> spec, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            var resultados = SpecificationEvaluator.GetQuery(query, spec);
            return await resultados.FirstOrDefaultAsync(cancellationToken);
        }

        public override Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 