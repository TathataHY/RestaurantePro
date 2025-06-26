using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteFidelizacion;

public class ObtenerReporteFidelizacionHandler : IRequestHandler<ObtenerReporteFidelizacionQuery, Result<ReporteFidelizacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerReporteFidelizacionHandler> _logger;

    public ObtenerReporteFidelizacionHandler(IApplicationDbContext context, ILogger<ObtenerReporteFidelizacionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ReporteFidelizacionDto>> Handle(ObtenerReporteFidelizacionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🎯 Generando reporte de fidelización desde {FechaInicio} hasta {FechaFin}", 
                request.FechaInicio.ToShortDateString(), request.FechaFin.ToShortDateString());

            // Query base de tarjetas
            var tarjetasQuery = _context.TarjetasFidelizacion.AsQueryable();

            var tarjetas = await tarjetasQuery.ToListAsync(cancellationToken);

            if (!tarjetas.Any())
            {
                return Result.Success(new ReporteFidelizacionDto
                {
                    TotalTarjetas = 0,
                    TarjetasActivas = 0,
                    TarjetasInactivas = 0,
                    TotalPuntosOtorgados = 0,
                    TotalPuntosCanjeados = 0,
                    PuntosDisponibles = 0
                });
            }

            var totalTarjetas = tarjetas.Count;
            var tarjetasActivas = tarjetas.Count(t => t.Estado == EstadoTarjeta.Activa);
            var tarjetasSuspendidas = tarjetas.Count(t => t.Estado == EstadoTarjeta.Suspendida);
            var tarjetasCanceladas = tarjetas.Count(t => t.Estado == EstadoTarjeta.Cancelada);

            // Estadísticas de puntos
            var totalPuntosAcumulados = tarjetas.Sum(t => t.PuntosAcumulados);
            var totalPuntosDisponibles = tarjetas.Sum(t => t.PuntosDisponibles);
            var totalPuntosCanjeados = totalPuntosAcumulados - totalPuntosDisponibles;

            // Distribución por nivel de fidelización
            var distribucionNiveles = tarjetas
                .GroupBy(t => t.NivelFidelizacion)
                .Select(g => new DistribucionNivelDto
                {
                    Nivel = g.Key.ToString(),
                    Cantidad = g.Count(),
                    Porcentaje = totalTarjetas > 0 ? (double)g.Count() / totalTarjetas * 100 : 0,
                    PuntosPromedio = g.Average(t => t.PuntosAcumulados)
                })
                .OrderByDescending(d => d.Cantidad)
                .ToList();

            // Top tarjetas por puntos acumulados
            var topTarjetasPuntos = tarjetas
                .OrderByDescending(t => t.PuntosAcumulados)
                .Take(10)
                .Select(t => new TarjetaTopPuntosDto
                {
                    TarjetaId = t.Id,
                    Codigo = t.Codigo,
                    ClienteId = t.ClienteId,
                    PuntosAcumulados = t.PuntosAcumulados,
                    PuntosDisponibles = t.PuntosDisponibles,
                    PuntosCanjeados = t.PuntosAcumulados - t.PuntosDisponibles,
                    NivelFidelizacion = t.NivelFidelizacion.ToString()
                })
                .ToList();

            // Top tarjetas por puntos canjeados
            var topTarjetasCanjeados = tarjetas
                .OrderByDescending(t => t.PuntosAcumulados - t.PuntosDisponibles)
                .Take(10)
                .Select(t => new TarjetaTopCanjeadosDto
                {
                    TarjetaId = t.Id,
                    Codigo = t.Codigo,
                    ClienteId = t.ClienteId,
                    PuntosCanjeados = t.PuntosAcumulados - t.PuntosDisponibles,
                    PuntosAcumulados = t.PuntosAcumulados,
                    NivelFidelizacion = t.NivelFidelizacion.ToString()
                })
                .ToList();

            var reporte = new ReporteFidelizacionDto
            {
                FechaGeneracion = DateTime.Now,
                TotalTarjetas = totalTarjetas,
                TarjetasActivas = tarjetasActivas,
                TarjetasInactivas = tarjetasSuspendidas + tarjetasCanceladas,
                TotalPuntosOtorgados = totalPuntosAcumulados,
                TotalPuntosCanjeados = totalPuntosCanjeados,
                PuntosDisponibles = totalPuntosDisponibles,
                DistribucionNiveles = distribucionNiveles,
                TopTarjetasPuntos = topTarjetasPuntos,
                TopTarjetasCanjeados = topTarjetasCanjeados
            };

            _logger.LogInformation("✅ Reporte de fidelización generado exitosamente. Total tarjetas: {TotalTarjetas}, Puntos disponibles: {PuntosDisponibles}", 
                totalTarjetas, totalPuntosDisponibles);

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de fidelización");
            return Result.Failure<ReporteFidelizacionDto>("Error al generar reporte de fidelización");
        }
    }
} 