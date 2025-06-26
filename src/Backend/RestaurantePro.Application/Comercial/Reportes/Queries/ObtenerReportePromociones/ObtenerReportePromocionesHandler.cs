using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReportePromociones;

public class ObtenerReportePromocionesHandler : IRequestHandler<ObtenerReportePromocionesQuery, Result<ReportePromocionesDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerReportePromocionesHandler> _logger;

    public ObtenerReportePromocionesHandler(IApplicationDbContext context, ILogger<ObtenerReportePromocionesHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ReportePromocionesDto>> Handle(ObtenerReportePromocionesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🎉 Generando reporte de promociones desde {FechaInicio} hasta {FechaFin}", 
                request.FechaInicio.ToShortDateString(), request.FechaFin.ToShortDateString());

            // Query base de promociones - filtrar por fechas de la promoción
            var promocionesQuery = _context.Promociones
                .Where(p => p.FechaInicio <= request.FechaFin && p.FechaFin >= request.FechaInicio);

            if (request.SoloActivas)
            {
                promocionesQuery = promocionesQuery.Where(p => p.Estado == Domain.Comercial.Promociones.Enums.EstadoPromocion.Activa);
            }

            var promociones = await promocionesQuery
                .ToListAsync(cancellationToken);

            if (!promociones.Any())
            {
                return Result.Success(new ReportePromocionesDto
                {
                    FechaInicio = request.FechaInicio,
                    FechaFin = request.FechaFin,
                    TotalPromociones = 0,
                    PromocionesActivas = 0,
                    PromocionesPausadas = 0,
                    PromocionesExpiradas = 0,
                    TotalDescuentosAplicados = 0,
                    TotalAplicaciones = 0
                });
            }

            var totalPromociones = promociones.Count;
            var promocionesActivas = promociones.Count(p => p.Estado == Domain.Comercial.Promociones.Enums.EstadoPromocion.Activa);
            var promocionesPausadas = promociones.Count(p => p.Estado == Domain.Comercial.Promociones.Enums.EstadoPromocion.Pausada);
            var promocionesExpiradas = promociones.Count(p => p.FechaFin < DateTime.UtcNow);

            // Usar datos reales de las promociones
            var totalDescuentosAplicados = promociones.Sum(p => p.ValorDescuento);
            var totalAplicaciones = promociones.Count; // Por ahora, una aplicación por promoción

            // Promociones más efectivas - usar datos reales
            var promocionesMasEfectivas = promociones
                .OrderByDescending(p => p.ValorDescuento)
                .Take(10)
                .Select(p => new PromocionEfectivaDto
                {
                    PromocionId = p.Id,
                    NombrePromocion = p.Nombre,
                    Codigo = p.Codigo,
                    NumeroAplicaciones = 1, // Por ahora, una aplicación por promoción
                    TotalDescuentos = p.ValorDescuento,
                    DescuentoPromedio = p.ValorDescuento,
                    PorcentajeEfectividad = 100, // Por ahora, 100% efectividad
                    FechaInicio = p.FechaInicio,
                    FechaFin = p.FechaFin
                })
                .ToList();

            // Promociones por tipo - usar datos reales
            var promocionesPorTipo = promociones
                .GroupBy(p => p.Tipo.ToString())
                .Select(g => new PromocionPorTipoDto
                {
                    TipoPromocion = g.Key,
                    Cantidad = g.Count(),
                    TotalDescuentos = g.Sum(p => p.ValorDescuento),
                    TotalAplicaciones = g.Count(),
                    EfectividadPromedio = 100 // Por ahora, 100% efectividad
                })
                .ToList();

            var reporte = new ReportePromocionesDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                TotalPromociones = totalPromociones,
                PromocionesActivas = promocionesActivas,
                PromocionesPausadas = promocionesPausadas,
                PromocionesExpiradas = promocionesExpiradas,
                TotalDescuentosAplicados = totalDescuentosAplicados,
                TotalAplicaciones = totalAplicaciones,
                PromocionesMasEfectivas = promocionesMasEfectivas,
                PromocionesPorTipo = promocionesPorTipo
            };

            _logger.LogInformation("✅ Reporte de promociones generado exitosamente. Total: {TotalPromociones}, Activas: {PromocionesActivas}", 
                totalPromociones, promocionesActivas);

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de promociones");
            return Result.Failure<ReportePromocionesDto>("Error al generar reporte de promociones");
        }
    }
} 