using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using RestaurantePro.Infrastructure.Persistence.Context;
using RestaurantePro.Infrastructure.Persistence.Repositories.Common;

namespace RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones
{
    /// <summary>
    /// Implementación del repositorio de preparaciones diarias
    /// </summary>
    public class PreparacionRepository : GenericRepository<PreparacionDiaria>, IPreparacionRepository
    {
        private readonly IDateTimeService _dateTimeService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public PreparacionRepository(
            ApplicationDbContext context,
            IDateTimeService dateTimeService) : base(context)
        {
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Obtiene todas las preparaciones del día actual
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now.Date;
            
            return await _context.Preparaciones
                .Where(p => p.FechaCreacion.Date == fechaActual)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones disponibles para un producto específico
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDisponiblesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            return await _context.Preparaciones
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
            return await _context.Preparaciones
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
            
            return await _context.Preparaciones
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
            return await _context.Preparaciones
                .Where(p => p.ChefId == chefId)
                .ToListAsync(cancellationToken);
        }
        
        /// <summary>
        /// Obtiene estadísticas de preparaciones del día actual
        /// </summary>
        public async Task<EstadisticasPreparaciones> ObtenerEstadisticasDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            var preparaciones = await _context.Preparaciones
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
    }
} 