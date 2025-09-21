using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Operaciones.Preparaciones.Queries.ObtenerMenuDelDia
{
    /// <summary>
    /// Manejador para la query de obtener el menú del día (solo preparaciones disponibles)
    /// </summary>
    public class ObtenerMenuDelDiaQueryHandler : IRequestHandler<ObtenerMenuDelDiaQuery, Result<List<PreparacionDiariaDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ObtenerMenuDelDiaQueryHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ObtenerMenuDelDiaQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ObtenerMenuDelDiaQueryHandler> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Maneja la query para obtener el menú del día
        /// </summary>
        public async Task<Result<List<PreparacionDiariaDto>>> Handle(
            ObtenerMenuDelDiaQuery request,
            CancellationToken cancellationToken)
        {
            var fechaObjetivo = request.Fecha?.Date ?? DateTime.Today;
            var fechaLimiteInferior = fechaObjetivo.AddDays(-2); // Solo últimas 3 días operativos (ayer, hoy, mañana)
            var fechaLimiteSuperior = fechaObjetivo.AddDays(2); // Incluir preparaciones de mañana y pasado mañana
            var ahora = DateTime.Now;
            var limite = request.Limite ?? 10;
            
            _logger.LogInformation("🍽️ Obteniendo menú del día - Rango operativo: {FechaDesde} a {FechaHasta}, Solo preparaciones vigentes", 
                fechaLimiteInferior.ToString("yyyy-MM-dd"), fechaLimiteSuperior.ToString("yyyy-MM-dd"));

            try
            {
                // Query específico para menú del día con filtros inteligentes
                
                var menuDelDia = await _context.PreparacionesDiarias
                    .Where(p => !p.EstaEliminado) // 🚫 No eliminadas
                    .Where(p => p.FechaVencimiento > ahora) // ⏰ PRIMERO: Solo NO vencidas
                    .Where(p => p.Estado == EstadoPreparacion.Disponible || p.Estado == EstadoPreparacion.Preparando) // ✅ SEGUNDO: Estados servibles
                    .Where(p => p.FechaVencimiento.Date >= fechaLimiteInferior && p.FechaVencimiento.Date <= fechaLimiteSuperior) // 📅 TERCERO: Solo 5 días operativos
                    .OrderBy(p => p.FechaVencimiento) // ⏰ Ordenar por proximidad de vencimiento (urgente primero)
                    .Take(limite) // 🔢 ÚLTIMO: Tomar las primeras N válidas
                    .Select(p => new PreparacionDiariaDto
                    {
                        Id = p.Id,
                        ProductoId = p.ProductoId,
                        ChefId = p.ChefId,
                        CantidadPreparada = p.CantidadPreparada,
                        CantidadDisponible = p.CantidadDisponible,
                        FechaVencimiento = p.FechaVencimiento,
                        Observaciones = p.Observaciones ?? string.Empty,
                        FechaPreparacion = p.FechaPreparacion,
                        Estado = p.Estado,
                        // Nombres mediante joins
                        NombreProducto = _context.Productos
                            .Where(prod => prod.Id == p.ProductoId)
                            .Select(prod => prod.Nombre)
                            .FirstOrDefault() ?? string.Empty,
                        NombreChef = (
                            _context.Usuarios
                                .Where(u => u.Id == p.ChefId)
                                .Select(u => u.NombreCompleto)
                                .FirstOrDefault()
                            ?? _context.Usuarios
                                .Where(u => u.IdentityId == p.ChefId.ToString())
                                .Select(u => u.NombreCompleto)
                                .FirstOrDefault()
                        ) ?? string.Empty
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("✅ Menú del día obtenido exitosamente: {Cantidad} preparaciones operativas vigentes (Rango: {FechaDesde}-{FechaHasta}, Hora: {Hora})", 
                    menuDelDia.Count, fechaLimiteInferior.ToString("dd/MM"), fechaLimiteSuperior.ToString("dd/MM"), ahora.ToString("HH:mm:ss"));

                return Result.Success(menuDelDia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener menú del día");
                return Result.Failure<List<PreparacionDiariaDto>>($"Error al obtener menú del día: {ex.Message}");
            }
        }
    }
}
