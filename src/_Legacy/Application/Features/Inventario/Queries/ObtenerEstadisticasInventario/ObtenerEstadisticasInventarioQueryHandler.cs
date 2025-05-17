using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Inventario.Dtos;
using RestaurantePro.Domain.Enums;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerEstadisticasInventario
{
    public class ObtenerEstadisticasInventarioQueryHandler : IRequestHandler<ObtenerEstadisticasInventarioQuery, EstadisticasInventarioDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;

        public ObtenerEstadisticasInventarioQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IDateTime dateTime)
        {
            _context = context;
            _mapper = mapper;
            _dateTime = dateTime;
        }

        public async Task<EstadisticasInventarioDto> Handle(ObtenerEstadisticasInventarioQuery request, CancellationToken cancellationToken)
        {
            // Establecer fecha inicio para filtrar datos
            var fechaInicio = _dateTime.Now.AddDays(-request.PeriodoDias);

            // Obtener todos los ítems del inventario
            var inventario = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .ToListAsync(cancellationToken);

            // Obtener movimientos de inventario en el periodo
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Ingrediente)
                .Where(m => m.Fecha >= fechaInicio)
                .ToListAsync(cancellationToken);

            // Obtener órdenes de compra pendientes
            var ordenesPendientes = await _context.OrdenesCompra
                .Where(o => o.Estado == EstadoOrdenCompra.Pendiente || o.Estado == EstadoOrdenCompra.Parcial)
                .ToListAsync(cancellationToken);

            // 1. Valor total del inventario
            decimal valorTotalInventario = inventario.Sum(i => i.CantidadDisponible * i.CostoUnitario);

            // 2. Productos con stock bajo
            int productosBajoStock = inventario.Count(i => i.CantidadDisponible <= i.CantidadMinima);

            // 3. Productos con mayor rotación (salidas y mermas)
            var productosConSalida = movimientos
                .Where(m => m.TipoMovimiento == TipoMovimiento.Salida || m.TipoMovimiento == TipoMovimiento.Merma)
                .GroupBy(m => new { m.IngredienteId, m.Ingrediente.Nombre, m.UnidadMedida })
                .Select(g => new ProductoRotacionDto
                {
                    IngredienteId = g.Key.IngredienteId,
                    NombreIngrediente = g.Key.Nombre,
                    UnidadMedida = g.Key.UnidadMedida,
                    CantidadConsumida = g.Sum(m => m.Cantidad),
                    ValorConsumido = g.Sum(m => m.CostoTotal),
                    NumeroMovimientos = g.Count()
                })
                .OrderByDescending(p => p.CantidadConsumida)
                .Take(10)
                .ToList();

            // 4. Productos sin movimiento en el periodo
            var idsConMovimiento = movimientos.Select(m => m.IngredienteId).Distinct().ToList();
            var productosSinMovimiento = inventario
                .Where(i => !idsConMovimiento.Contains(i.IngredienteId))
                .Select(i => _mapper.Map<InventarioDto>(i))
                .ToList();

            // 5. Estadísticas de movimientos por tipo
            var movimientosPorTipo = movimientos
                .GroupBy(m => m.TipoMovimiento)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => new MovimientoEstadisticaDto
                    {
                        TipoMovimiento = g.Key.ToString(),
                        CantidadMovimientos = g.Count(),
                        ValorTotal = g.Sum(m => m.CostoTotal)
                    }
                );

            // Crear el objeto de estadísticas
            var estadisticas = new EstadisticasInventarioDto
            {
                ValorTotalInventario = valorTotalInventario,
                ProductosBajoStock = productosBajoStock,
                ProductosMayorRotacion = productosConSalida,
                ProductosSinMovimiento = productosSinMovimiento,
                OrdenesPendientes = ordenesPendientes.Count,
                ValorOrdenesPendientes = ordenesPendientes.Sum(o => o.Total),
                MovimientosPorTipo = movimientosPorTipo,
                UltimaActualizacion = _dateTime.Now
            };

            return estadisticas;
        }
    }
} 