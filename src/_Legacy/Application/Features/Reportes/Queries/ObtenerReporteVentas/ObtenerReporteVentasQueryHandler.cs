using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Features.Reportes.Dtos;
using RestaurantePro.Application.Interfaces;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Reportes.Queries.ObtenerReporteVentas
{
    public class ObtenerReporteVentasQueryHandler : IRequestHandler<ObtenerReporteVentasQuery, ReporteVentasDto>
    {
        private readonly IApplicationDbContext _context;

        public ObtenerReporteVentasQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReporteVentasDto> Handle(ObtenerReporteVentasQuery request, CancellationToken cancellationToken)
        {
            // Asegurarse de que la fecha fin incluya todo el día
            var fechaFin = request.FechaFin.Date.AddDays(1).AddTicks(-1);
            
            // Consultar los pagos en el rango de fechas
            var pagos = await _context.Pagos
                .Include(p => p.Comanda)
                .ThenInclude(c => c.DetallesComanda)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(p => p.Fecha >= request.FechaInicio.Date && 
                           p.Fecha <= fechaFin &&
                           p.Estado == EstadoPago.Completado)
                .ToListAsync(cancellationToken);

            // Agrupar pagos por día
            var ventasPorDia = pagos
                .GroupBy(p => p.Fecha.Date)
                .Select(g => new VentaPorDiaDto
                {
                    Fecha = g.Key,
                    Total = g.Sum(p => p.Monto),
                    CantidadComandas = g.Select(p => p.ComandaId).Distinct().Count()
                })
                .OrderBy(v => v.Fecha)
                .ToList();

            // Agrupar pagos por método de pago
            var totalVentas = pagos.Sum(p => p.Monto);
            var ventasPorMetodoPago = pagos
                .GroupBy(p => p.FormaPago)
                .Select(g => new VentaPorMetodoPagoDto
                {
                    MetodoPago = g.Key.ToString(),
                    Total = g.Sum(p => p.Monto),
                    Porcentaje = totalVentas > 0 ? (g.Sum(p => p.Monto) / totalVentas) * 100 : 0
                })
                .OrderByDescending(v => v.Total)
                .ToList();

            // Obtener comandas únicas para calcular cantidad
            var comandasIds = pagos.Select(p => p.ComandaId).Distinct().ToList();
            var comandas = await _context.Comandas
                .Include(c => c.DetallesComanda)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .Where(c => comandasIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            // Productos más vendidos
            var detallesComanda = comandas.SelectMany(c => c.DetallesComanda).ToList();
            
            if (request.CategoriaId.HasValue)
            {
                detallesComanda = detallesComanda
                    .Where(d => d.Producto.CategoriaId == request.CategoriaId.Value)
                    .ToList();
            }

            var productosMasVendidos = detallesComanda
                .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, Categoria = d.Producto.Categoria.Nombre })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    Categoria = g.Key.Categoria,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Total = g.Sum(d => d.PrecioUnitario * d.Cantidad)
                })
                .OrderByDescending(p => p.Cantidad)
                .Take(request.TopProductos ?? 10)
                .ToList();

            // Construir el reporte
            return new ReporteVentasDto
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                VentasTotales = totalVentas,
                CantidadComandas = comandas.Count,
                TicketPromedio = comandas.Count > 0 ? totalVentas / comandas.Count : 0,
                VentasPorDia = ventasPorDia,
                VentasPorMetodoPago = ventasPorMetodoPago,
                ProductosMasVendidos = productosMasVendidos
            };
        }
    }
} 