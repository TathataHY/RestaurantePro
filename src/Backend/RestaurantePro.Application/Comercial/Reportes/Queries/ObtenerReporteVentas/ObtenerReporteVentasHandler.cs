using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Comercial.Reportes.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerReporteVentas;

public class ObtenerReporteVentasHandler : IRequestHandler<ObtenerReporteVentasQuery, Result<ReporteVentasDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ObtenerReporteVentasHandler> _logger;

    public ObtenerReporteVentasHandler(IApplicationDbContext context, ILogger<ObtenerReporteVentasHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<ReporteVentasDto>> Handle(ObtenerReporteVentasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validaciones de entrada
            if (request.FechaInicio > request.FechaFin)
            {
                return Result.Failure<ReporteVentasDto>("La fecha de inicio debe ser anterior o igual a la fecha fin");
            }

            if (request.FechaInicio > DateTime.Now)
            {
                return Result.Failure<ReporteVentasDto>("La fecha de inicio no puede ser futura");
            }

            _logger.LogInformation("📊 Generando reporte de ventas desde {FechaInicio} hasta {FechaFin}", 
                request.FechaInicio.ToShortDateString(), request.FechaFin.ToShortDateString());

            // Obtener facturas en el rango de fechas
            var facturasQuery = _context.Facturas
                .Where(f => f.FechaCreacion >= request.FechaInicio && f.FechaCreacion <= request.FechaFin);

            if (!request.IncluirCanceladas)
            {
                facturasQuery = facturasQuery.Where(f => f.Estado != Domain.Comercial.Facturacion.Enums.EstadoFactura.Anulada);
            }

            var facturas = await facturasQuery
                .Include(f => f.Detalles)
                .ToListAsync(cancellationToken);

            if (!facturas.Any())
            {
                return Result.Success(new ReporteVentasDto
                {
                    FechaInicio = request.FechaInicio,
                    FechaFin = request.FechaFin,
                    TotalVentas = 0,
                    TotalTransacciones = 0,
                    PromedioTicket = 0
                });
            }

            var totalVentas = facturas.Sum(f => f.Total);
            var totalTransacciones = facturas.Count;
            var promedioTicket = totalTransacciones > 0 ? totalVentas / totalTransacciones : 0;

            // Ventas por día
            var ventasPorDia = facturas
                .GroupBy(f => f.FechaCreacion.Date)
                .Select(g => new VentaDiariaDto
                {
                    Fecha = g.Key,
                    TotalVentas = g.Sum(f => f.Total),
                    NumeroTransacciones = g.Count(),
                    PromedioTicket = g.Count() > 0 ? g.Sum(f => f.Total) / g.Count() : 0
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            // Productos más vendidos (simulado - en un caso real se calcularía con los detalles)
            var productosMasVendidos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    ProductoId = Guid.NewGuid(),
                    NombreProducto = "Producto Test",
                    CantidadVendida = 10,
                    TotalVendido = 100m,
                    PorcentajeDelTotal = 50
                }
            };

            // Ventas por segmento (simulado)
            var ventasPorSegmento = new List<SegmentoVentasDto>
            {
                new SegmentoVentasDto
                {
                    Segmento = "General",
                    TotalVentas = totalVentas,
                    NumeroTransacciones = totalTransacciones,
                    PorcentajeDelTotal = 100
                }
            };

            var reporte = new ReporteVentasDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                TotalVentas = totalVentas,
                TotalTransacciones = totalTransacciones,
                PromedioTicket = promedioTicket,
                VentasPorDia = ventasPorDia,
                ProductosMasVendidos = productosMasVendidos,
                VentasPorSegmento = ventasPorSegmento
            };

            _logger.LogInformation("✅ Reporte de ventas generado exitosamente. Total: {TotalVentas:C}, Transacciones: {TotalTransacciones}", 
                totalVentas, totalTransacciones);

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando reporte de ventas");
            return Result.Failure<ReporteVentasDto>("Error al generar reporte de ventas");
        }
    }
} 