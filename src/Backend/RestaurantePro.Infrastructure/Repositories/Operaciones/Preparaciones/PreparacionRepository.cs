using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;
using RestaurantePro.Infrastructure.Data;
using RestaurantePro.Infrastructure.Repositories.Common;

namespace RestaurantePro.Infrastructure.Repositories.Operaciones.Preparaciones
{
    /// <summary>
    /// Implementación del repositorio de preparaciones diarias
    /// </summary>
    public class PreparacionRepository : RepositoryBase<PreparacionDiaria>, IPreparacionRepository
    {
        private readonly ILogger<PreparacionRepository> _logger;
        private readonly IDateTimeService _dateTimeService;

        public PreparacionRepository(
            ApplicationDbContext dbContext,
            ILogger<PreparacionRepository> logger,
            IDateTimeService dateTimeService) 
            : base(dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        /// <summary>
        /// Obtiene todas las preparaciones del día actual
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now.Date;
            var fechaSiguiente = fechaActual.AddDays(1);

            _logger.LogDebug("Obteniendo preparaciones del día {Fecha}", fechaActual.ToShortDateString());

            return await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaPreparacion >= fechaActual && p.FechaPreparacion < fechaSiguiente)
                .OrderByDescending(p => p.FechaPreparacion)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones disponibles para un producto específico
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPreparacionesDisponiblesPorProductoAsync(Guid productoId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo preparaciones disponibles para producto {ProductoId}", productoId);

            return await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.ProductoId == productoId && 
                           (p.Estado == EstadoPreparacion.Disponible || 
                            p.Estado == EstadoPreparacion.PorVencer) &&
                            p.CantidadDisponible > 0)
                .OrderBy(p => p.FechaVencimiento) // Las más próximas a vencer primero (FIFO)
                .ThenBy(p => p.FechaPreparacion)  // Las más antiguas primero
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las preparaciones por su estado
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorEstadoAsync(EstadoPreparacion estado, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo preparaciones con estado {Estado}", estado);

            return await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaPreparacion)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones que vencen en un rango de tiempo específico
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorVencerAsync(int horasAnticipacion = 2, CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now;
            var fechaLimite = fechaActual.AddHours(horasAnticipacion);

            _logger.LogDebug("Obteniendo preparaciones por vencer en las próximas {Horas} horas", horasAnticipacion);

            return await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaVencimiento.HasValue && 
                           p.FechaVencimiento.Value <= fechaLimite &&
                           p.Estado == EstadoPreparacion.Disponible)
                .OrderBy(p => p.FechaVencimiento)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene preparaciones por chef
        /// </summary>
        public async Task<IEnumerable<PreparacionDiaria>> ObtenerPorChefAsync(Guid chefId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Obteniendo preparaciones realizadas por chef {ChefId}", chefId);

            return await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.ChefId == chefId)
                .OrderByDescending(p => p.FechaPreparacion)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene estadísticas de preparaciones del día actual
        /// </summary>
        public async Task<EstadisticasPreparaciones> ObtenerEstadisticasDelDiaAsync(CancellationToken cancellationToken = default)
        {
            var fechaActual = _dateTimeService.Now.Date;
            var fechaSiguiente = fechaActual.AddDays(1);

            _logger.LogDebug("Obteniendo estadísticas de preparaciones del día {Fecha}", fechaActual.ToShortDateString());

            // Obtener todas las preparaciones del día
            var preparaciones = await DbContext.Set<PreparacionDiaria>()
                .Where(p => p.FechaPreparacion >= fechaActual && p.FechaPreparacion < fechaSiguiente)
                .ToListAsync(cancellationToken);

            if (!preparaciones.Any())
            {
                return new EstadisticasPreparaciones();
            }

            // Calcular estadísticas
            var disponibles = preparaciones.Count(p => p.Estado == EstadoPreparacion.Disponible);
            var porVencer = preparaciones.Count(p => p.Estado == EstadoPreparacion.PorVencer);
            var agotadas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Agotada);
            var vencidas = preparaciones.Count(p => p.Estado == EstadoPreparacion.Vencida);
            
            var cantidadPreparada = preparaciones.Sum(p => p.CantidadPreparada);
            var cantidadDisponible = preparaciones.Sum(p => p.CantidadDisponible);
            var cantidadConsumida = cantidadPreparada - cantidadDisponible;
            var cantidadDesperdiciada = preparaciones
                .Where(p => p.Estado == EstadoPreparacion.Vencida)
                .Sum(p => p.CantidadPreparada - p.CantidadDisponible);

            // Calcular porcentaje de eficiencia (consumido/total sin desperdicio)
            decimal porcentajeEficiencia = 0;
            if (cantidadPreparada > 0)
            {
                porcentajeEficiencia = (decimal)(cantidadConsumida - cantidadDesperdiciada) / cantidadPreparada * 100;
                porcentajeEficiencia = Math.Max(0, porcentajeEficiencia); // No permitir porcentajes negativos
                porcentajeEficiencia = Math.Round(porcentajeEficiencia, 2); // Redondear a 2 decimales
            }

            return new EstadisticasPreparaciones
            {
                TotalPreparaciones = preparaciones.Count,
                PreparacionesDisponibles = disponibles,
                PreparacionesPorVencer = porVencer,
                PreparacionesAgotadas = agotadas,
                PreparacionesVencidas = vencidas,
                CantidadTotalPreparada = cantidadPreparada,
                CantidadTotalConsumida = cantidadConsumida,
                CantidadDesperdiciada = cantidadDesperdiciada,
                PorcentajeEficiencia = porcentajeEficiencia
            };
        }
    }
} 