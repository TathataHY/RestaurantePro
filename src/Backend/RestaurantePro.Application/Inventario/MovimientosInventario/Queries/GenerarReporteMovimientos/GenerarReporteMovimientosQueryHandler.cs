using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.MovimientosInventario.DTOs;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

namespace RestaurantePro.Application.Inventario.MovimientosInventario.Queries.GenerarReporteMovimientos;

public class GenerarReporteMovimientosQueryHandler : IRequestHandler<GenerarReporteMovimientosQuery, Result<ReporteMovimientosDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GenerarReporteMovimientosQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<ReporteMovimientosDto>> Handle(GenerarReporteMovimientosQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Como MovimientoInventario es una owned entity, necesitamos consultar a través de Ingredientes
            var query = _context.Ingredientes
                .Include(i => i.Movimientos)
                .Include(i => i.ProveedorPrincipal)
                .AsQueryable();

            // Aplicar filtros
            if (request.TipoMovimiento.HasValue)
            {
                query = query.Where(i => i.Movimientos.Any(m => m.TipoMovimiento == request.TipoMovimiento.Value));
            }

            if (request.IngredienteId.HasValue)
            {
                query = query.Where(i => i.Id == request.IngredienteId.Value);
            }

            // Obtener ingredientes con sus movimientos
            var ingredientes = await query.ToListAsync(cancellationToken);

            // Extraer y aplanar todos los movimientos del período
            var movimientos = ingredientes
                .SelectMany(i => i.Movimientos
                    .Where(m => m.Fecha >= request.FechaInicio && m.Fecha <= request.FechaFin)
                    .Select(m => new
                    {
                        Movimiento = m,
                        Ingrediente = i
                    }))
                .ToList();

            // Aplicar filtros adicionales
            if (request.TipoMovimiento.HasValue)
            {
                movimientos = movimientos.Where(x => x.Movimiento.TipoMovimiento == request.TipoMovimiento.Value).ToList();
            }

            // Crear el reporte
            var reporte = new ReporteMovimientosDto
            {
                PeriodoReporte = $"{request.FechaInicio:dd/MM/yyyy} - {request.FechaFin:dd/MM/yyyy}",
                FechaGeneracion = DateTime.UtcNow,
                TotalMovimientos = movimientos.Count,
                Movimientos = movimientos.Select(x => new MovimientoInventarioDto
                {
                    Id = x.Movimiento.Id,
                    IngredienteId = x.Ingrediente.Id,
                    NombreIngrediente = x.Ingrediente.Nombre,
                    Tipo = x.Movimiento.TipoMovimiento,
                    Cantidad = x.Movimiento.Cantidad,
                    CostoUnitario = 0, // No disponible en la entidad actual
                    StockAnterior = 0, // No disponible en la entidad actual
                    StockResultante = x.Movimiento.CantidadFinal ?? 0,
                    Motivo = x.Movimiento.Motivo,
                    Observaciones = null, // No disponible en la entidad actual
                    UsuarioId = Guid.Empty, // No disponible en la entidad actual
                    NombreUsuario = "Sistema", // Placeholder
                    NumeroDocumento = null,
                    ProveedorId = x.Ingrediente.ProveedorPrincipal?.Id,
                    NombreProveedor = x.Ingrediente.ProveedorPrincipal?.Nombre ?? "",
                    FechaCreacion = x.Movimiento.Fecha,
                    FechaModificacion = x.Movimiento.FechaActualizacion,
                    CreadoPor = x.Movimiento.CreatedBy ?? "Sistema",
                    ModificadoPor = x.Movimiento.LastModifiedBy ?? "Sistema",
                    Activo = !x.Movimiento.EstaEliminado,
                    ColorTipo = x.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso ? "success" : "danger",
                    IconoTipo = x.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso ? "arrow-up" : "arrow-down",
                    FechaTexto = x.Movimiento.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    CantidadTexto = $"{x.Movimiento.Cantidad} {x.Ingrediente.UnidadMedida}"
                }).ToList()
            };

            // Calcular totales
            reporte.TotalIngresos = movimientos
                .Where(m => m.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso)
                .Sum(m => m.Movimiento.Cantidad);

            reporte.TotalEgresos = movimientos
                .Where(m => m.Movimiento.TipoMovimiento == TipoMovimientoInventario.Egreso)
                .Sum(m => m.Movimiento.Cantidad);

            reporte.BalanceNeto = reporte.TotalIngresos - reporte.TotalEgresos;
            reporte.ValorTotalMovimientos = 0; // No disponible en la entidad actual

            // Generar resumen por tipo si se solicita
            if (request.IncluirResumenPorTipo)
            {
                reporte.ResumenPorTipo = movimientos
                    .GroupBy(m => m.Movimiento.TipoMovimiento)
                    .Select(g => new ResumenPorTipoDto
                    {
                        TipoMovimiento = g.Key.ToString(),
                        CantidadMovimientos = g.Count(),
                        CantidadTotal = g.Sum(m => m.Movimiento.Cantidad),
                        ValorTotal = 0, // No disponible en la entidad actual
                        PorcentajeDelTotal = movimientos.Count > 0 ? (decimal)g.Count() / movimientos.Count * 100 : 0
                    })
                    .ToList();
            }

            // Generar resumen por ingrediente si se solicita
            if (request.IncluirResumenPorIngrediente)
            {
                reporte.ResumenPorIngrediente = movimientos
                    .GroupBy(m => new { m.Ingrediente.Id, m.Ingrediente.Nombre })
                    .Select(g => new ResumenPorIngredienteDto
                    {
                        IngredienteId = g.Key.Id,
                        NombreIngrediente = g.Key.Nombre,
                        CantidadMovimientos = g.Count(),
                        CantidadTotal = g.Sum(m => m.Movimiento.Cantidad),
                        ValorTotal = 0, // No disponible en la entidad actual
                        StockActual = g.First().Ingrediente.Stock
                    })
                    .ToList();
            }

            // Generar análisis de tendencias si se solicita
            if (request.IncluirTendencias)
            {
                var analisisTendencias = await GenerarAnalisisTendencias(movimientos.Cast<dynamic>().ToList(), request.FechaInicio, request.FechaFin, cancellationToken);
                reporte.AnalisisTendencias = analisisTendencias;
            }

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            return Result.Failure<ReporteMovimientosDto>($"Error al generar reporte: {ex.Message}");
        }
    }

    private async Task<AnalisisTendenciasDto> GenerarAnalisisTendencias(List<dynamic> movimientos, DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken)
    {
        var analisis = new AnalisisTendenciasDto();

        // Obtener movimientos del período anterior para comparación
        var diasPeriodo = (fechaFin - fechaInicio).Days;
        var fechaInicioAnterior = fechaInicio.AddDays(-diasPeriodo);
        var fechaFinAnterior = fechaInicio.AddDays(-1);

        var ingredientesAnteriores = await _context.Ingredientes
            .Include(i => i.Movimientos)
            .ToListAsync(cancellationToken);

        var movimientosAnteriores = ingredientesAnteriores
            .SelectMany(i => i.Movimientos
                .Where(m => m.Fecha >= fechaInicioAnterior && m.Fecha <= fechaFinAnterior)
                .Select(m => new { Movimiento = m, Ingrediente = i }))
            .ToList();

        // Calcular variación
        var totalActual = movimientos.Count;
        var totalAnterior = movimientosAnteriores.Count;

        if (totalAnterior > 0)
        {
            analisis.VariacionPorcentual = ((decimal)(totalActual - totalAnterior) / totalAnterior) * 100;
        }

        // Determinar tendencia general
        analisis.TendenciaGeneral = analisis.VariacionPorcentual switch
        {
            > 10 => "Aumento significativo",
            > 5 => "Aumento moderado",
            > -5 => "Estable",
            > -10 => "Disminución moderada",
            _ => "Disminución significativa"
        };

        // Generar tendencia diaria
        var fechas = Enumerable.Range(0, diasPeriodo + 1)
            .Select(i => fechaInicio.AddDays(i))
            .ToList();

        analisis.TendenciaDiaria = fechas.Select(fecha =>
        {
            var movimientosDia = movimientos.Where(m => m.Movimiento.Fecha.Date == fecha.Date).ToList();
            return new RestaurantePro.Application.Inventario.MovimientosInventario.DTOs.TendenciaDiariaDto
            {
                Fecha = fecha,
                CantidadMovimientos = movimientosDia.Count,
                CantidadTotal = movimientosDia.Sum(m => m.Movimiento.Cantidad),
                ValorTotal = 0 // No disponible en la entidad actual
            };
        }).ToList();

        // Generar recomendaciones
        analisis.Recomendaciones = GenerarRecomendaciones(movimientos, analisis.VariacionPorcentual);

        return analisis;
    }

    private List<string> GenerarRecomendaciones(List<dynamic> movimientos, decimal variacionPorcentual)
    {
        var recomendaciones = new List<string>();

        if (variacionPorcentual > 20)
        {
            recomendaciones.Add("Considerar revisar los procesos de control de inventario");
        }

        if (movimientos.Count > 100)
        {
            recomendaciones.Add("Evaluar la posibilidad de automatizar algunos procesos de inventario");
        }

        var egresos = movimientos.Where(m => m.Movimiento.TipoMovimiento == TipoMovimientoInventario.Egreso).Count();
        var ingresos = movimientos.Where(m => m.Movimiento.TipoMovimiento == TipoMovimientoInventario.Ingreso).Count();

        if (egresos > ingresos * 1.5m)
        {
            recomendaciones.Add("Revisar el consumo de ingredientes para optimizar las compras");
        }

        return recomendaciones;
    }
} 