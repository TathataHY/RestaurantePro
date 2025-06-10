using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerEstadisticasPreparaciones
{
    /// <summary>
    /// Manejador para la consulta de obtener estadísticas de preparaciones
    /// </summary>
    public class ObtenerEstadisticasPreparacionesQueryHandler : IRequestHandler<ObtenerEstadisticasPreparacionesQuery, EstadisticasPreparacionesDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObtenerEstadisticasPreparacionesQueryHandler(
            IApplicationDbContext context,
            IDateTimeService dateTimeService)
        {
            _context = context;
            _dateTimeService = dateTimeService;
        }

        /// <summary>
        /// Maneja la consulta para obtener estadísticas de preparaciones
        /// </summary>
        public async Task<EstadisticasPreparacionesDto> Handle(
            ObtenerEstadisticasPreparacionesQuery request,
            CancellationToken cancellationToken)
        {
            var fechaActual = _dateTimeService.Now;
            
            // Obtenemos todas las preparaciones
            var preparaciones = await Task.FromResult(_context.Preparaciones.ToList());
            
            // Calculamos estadísticas a partir de los datos reales
            var totalPreparaciones = preparaciones.Count;
            
            // Identificamos las preparaciones por vencer primero
            var preparacionesPorVencer = preparaciones
                .Where(p => p.Estado == EstadoPreparacion.Disponible && 
                           p.FechaVencimiento <= fechaActual.AddHours(5) && 
                           p.FechaVencimiento > fechaActual)
                .ToList();
                
            // Contamos las preparaciones disponibles excluyendo las que están por vencer
            var preparacionesDisponibles = preparaciones
                .Count(p => p.Estado == EstadoPreparacion.Disponible && 
                            !preparacionesPorVencer.Any(pv => pv.Id == p.Id));
                
            var preparacionesAgotadas = preparaciones
                .Count(p => p.Estado == EstadoPreparacion.Agotada);
                
            var preparacionesVencidas = preparaciones
                .Count(p => p.Estado == EstadoPreparacion.Vencida);
            
            // Calculamos cantidades
            var cantidadTotalPreparada = preparaciones.Sum(p => p.CantidadPreparada);
            var cantidadDisponible = preparaciones.Sum(p => p.CantidadDisponible);
            var cantidadConsumida = preparaciones.Sum(p => p.CantidadPreparada - p.CantidadDisponible);
            
            // La cantidad desperdiciada son los productos vencidos que no se consumieron
            var cantidadDesperdiciada = preparaciones
                .Where(p => p.Estado == EstadoPreparacion.Vencida)
                .Sum(p => p.CantidadDisponible);
            
            // Calculamos el porcentaje de eficiencia
            var porcentajeEficiencia = cantidadTotalPreparada > 0
                ? (decimal)cantidadConsumida / cantidadTotalPreparada * 100
                : 0;
            
            // Creamos y devolvemos el DTO con las estadísticas
            var estadisticas = new EstadisticasPreparacionesDto
            {
                TotalPreparaciones = totalPreparaciones,
                PreparacionesDisponibles = preparacionesDisponibles,
                PreparacionesPorVencer = preparacionesPorVencer.Count(),
                PreparacionesAgotadas = preparacionesAgotadas,
                PreparacionesVencidas = preparacionesVencidas,
                
                CantidadTotalPreparada = cantidadTotalPreparada,
                CantidadDisponible = cantidadDisponible,
                CantidadConsumida = cantidadConsumida,
                CantidadDesperdiciada = cantidadDesperdiciada,
                
                PorcentajeEficiencia = porcentajeEficiencia
            };
            
            return estadisticas;
        }
    }
} 