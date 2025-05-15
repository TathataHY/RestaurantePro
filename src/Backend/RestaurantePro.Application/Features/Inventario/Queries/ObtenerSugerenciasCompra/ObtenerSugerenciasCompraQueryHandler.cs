using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Inventario.Dtos;
using RestaurantePro.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Features.Inventario.Queries.ObtenerSugerenciasCompra
{
    public class ObtenerSugerenciasCompraQueryHandler : IRequestHandler<ObtenerSugerenciasCompraQuery, List<SugerenciaCompraDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IDateTime _dateTime;

        public ObtenerSugerenciasCompraQueryHandler(
            IApplicationDbContext context,
            IDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<List<SugerenciaCompraDto>> Handle(ObtenerSugerenciasCompraQuery request, CancellationToken cancellationToken)
        {
            // Establecer fecha inicio para filtrar datos históricos
            var fechaInicio = _dateTime.Now.AddDays(-request.PeriodoHistoricoDias);

            // Obtener todos los ítems del inventario
            var inventario = await _context.Inventarios
                .Include(i => i.Ingrediente)
                .ToListAsync(cancellationToken);

            // Si solo queremos ingredientes bajo mínimo, filtrar
            if (request.SoloBajoMinimo)
            {
                inventario = inventario.Where(i => i.CantidadDisponible <= i.CantidadMinima).ToList();
            }

            // Obtener movimientos de salida en el periodo histórico
            var movimientosSalida = await _context.MovimientosInventario
                .Where(m => m.Fecha >= fechaInicio && 
                           (m.TipoMovimiento == TipoMovimiento.Salida || m.TipoMovimiento == TipoMovimiento.Merma))
                .ToListAsync(cancellationToken);

            // Obtener relaciones con proveedores
            var proveedoresIngredientes = await _context.ProveedoresIngredientes
                .Include(pi => pi.Proveedor)
                .ToListAsync(cancellationToken);

            // Calcular consumo diario promedio por ingrediente
            var consumoDiario = movimientosSalida
                .GroupBy(m => m.IngredienteId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(m => m.Cantidad) / request.PeriodoHistoricoDias
                );

            // Lista para almacenar las sugerencias
            var sugerencias = new List<SugerenciaCompraDto>();

            // Procesar cada ítem de inventario
            foreach (var item in inventario)
            {
                // Obtener consumo diario promedio (si no hay datos históricos, usar un valor por defecto)
                decimal consumoPromedioDiario = consumoDiario.ContainsKey(item.IngredienteId)
                    ? consumoDiario[item.IngredienteId]
                    : (item.CantidadOptima > 0 ? item.CantidadOptima / 30 : 0);

                // Si no hay consumo y solo se requieren ingredientes con movimiento, omitir
                if (consumoPromedioDiario <= 0)
                {
                    continue;
                }

                // Proyectar consumo para el periodo futuro
                decimal consumoProyectado = consumoPromedioDiario * request.PeriodoProyeccionDias;

                // Calcular cantidad a comprar teniendo en cuenta el stock actual y añadiendo un margen de seguridad
                decimal cantidadNecesaria = consumoProyectado - item.CantidadDisponible;
                if (cantidadNecesaria <= 0 && !request.SoloBajoMinimo)
                {
                    continue; // No se necesita comprar
                }

                // Añadir margen de seguridad
                decimal cantidadSugerida = cantidadNecesaria * (1 + request.PorcentajeReserva / 100);
                
                // Redondear hacia arriba para evitar decimales extraños
                cantidadSugerida = Math.Ceiling(cantidadSugerida * 100) / 100;

                // Determinar nivel de urgencia
                string urgencia;
                if (item.CantidadDisponible <= 0)
                {
                    urgencia = "Alta";
                }
                else if (item.CantidadDisponible < item.CantidadMinima)
                {
                    urgencia = "Media";
                }
                else
                {
                    urgencia = "Baja";
                }

                // Obtener información de proveedores para este ingrediente
                var proveedoresDelIngrediente = proveedoresIngredientes
                    .Where(pi => pi.IngredienteId == item.IngredienteId)
                    .ToList();

                // Obtener proveedor principal
                var proveedorPrincipal = proveedoresDelIngrediente
                    .FirstOrDefault(pi => pi.EsProveedorPrincipal);

                // Obtener proveedores alternativos
                var proveedoresAlternativos = proveedoresDelIngrediente
                    .Where(pi => !pi.EsProveedorPrincipal)
                    .Select(pi => new ProveedorAlternativoDto
                    {
                        ProveedorId = pi.ProveedorId,
                        NombreProveedor = pi.Proveedor?.Nombre ?? "Desconocido",
                        PrecioUnitario = pi.PrecioUnitario
                    })
                    .ToList();

                // Determinar costo estimado
                decimal costoUnitario = proveedorPrincipal?.PrecioUnitario ?? item.CostoUnitario;

                // Crear la sugerencia
                var sugerencia = new SugerenciaCompraDto
                {
                    IngredienteId = item.IngredienteId,
                    NombreIngrediente = item.Ingrediente?.Nombre ?? "Desconocido",
                    Categoria = item.Ingrediente?.Categoria ?? "Sin categoría",
                    StockActual = item.CantidadDisponible,
                    StockMinimo = item.CantidadMinima,
                    StockOptimo = item.CantidadOptima,
                    UnidadMedida = item.UnidadMedida,
                    ConsumoDiarioPromedio = consumoPromedioDiario,
                    ConsumoProyectado = consumoProyectado,
                    CantidadSugerida = cantidadSugerida,
                    Urgencia = urgencia,
                    CostoUnitarioEstimado = costoUnitario,
                    CostoTotalEstimado = cantidadSugerida * costoUnitario,
                    ProveedorPrincipalId = proveedorPrincipal?.ProveedorId,
                    NombreProveedorPrincipal = proveedorPrincipal?.Proveedor?.Nombre ?? "Sin proveedor principal",
                    ProveedoresAlternativos = proveedoresAlternativos
                };

                sugerencias.Add(sugerencia);
            }

            // Ordenar por urgencia y luego por nombre
            return sugerencias
                .OrderBy(s => s.Urgencia == "Alta" ? 0 : (s.Urgencia == "Media" ? 1 : 2))
                .ThenBy(s => s.NombreIngrediente)
                .ToList();
        }
    }
} 