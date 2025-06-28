using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;

public class ObtenerAnalisisInventarioQueryHandler : IRequestHandler<ObtenerAnalisisInventarioQuery, Result<AnalisisInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ObtenerAnalisisInventarioQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<AnalisisInventarioDto>> Handle(
        ObtenerAnalisisInventarioQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var fechaInicio = request.FechaDesde ?? DateTime.Today.AddDays(-30);
            var fechaFin = request.FechaHasta ?? DateTime.Today;

            // Obtener ingredientes
            var ingredientes = await _context.Ingredientes
                .Where(i => i.EstaActivo)
                .ToListAsync(cancellationToken);

            // Obtener movimientos del período
            var movimientos = await _context.MovimientosInventario
                .Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin)
                .ToListAsync(cancellationToken);

            // Construir análisis
            var analisis = new AnalisisInventarioDto
            {
                FechaGeneracion = _dateTimeService.Now,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalIngredientes = ingredientes.Count,
                ValorTotalInventario = ingredientes.Sum(i => i.Stock * i.CostoPromedio),
                ValorPromedioPorIngrediente = ingredientes.Count > 0 ? 
                    ingredientes.Sum(i => i.Stock * i.CostoPromedio) / ingredientes.Count : 0,
                IngredientesConMovimiento = movimientos.Select(m => m.IngredienteId).Distinct().Count(),
                AnalisisPorCategoria = ingredientes
                    .GroupBy(i => "Sin categoría") // TODO: Usar categoría real cuando esté disponible
                    .Select(g => new CategoriaAnalisisDto
                    {
                        Categoria = g.Key,
                        CantidadIngredientes = g.Count(),
                        ValorTotal = g.Sum(i => i.Stock * i.CostoPromedio),
                        PorcentajeDelTotal = ingredientes.Sum(i => i.Stock * i.CostoPromedio) > 0 ? 
                            (g.Sum(i => i.Stock * i.CostoPromedio) / ingredientes.Sum(i => i.Stock * i.CostoPromedio)) * 100 : 0,
                        StockBajo = g.Count(i => i.Stock <= i.StockMinimo && i.Stock > i.StockMinimo * 0.5m),
                        StockCritico = g.Count(i => i.Stock <= i.StockMinimo * 0.5m)
                    })
                    .ToList()
            };

            return Result.Success(analisis);
        }
        catch (Exception ex)
        {
            return Result.Failure<AnalisisInventarioDto>($"Error al generar análisis de inventario: {ex.Message}");
        }
    }

    // Métodos auxiliares
    private string DeterminarEstadoStock(decimal stockActual, decimal stockMinimo, decimal stockMaximo)
    {
        if (stockActual <= 0) return "SinStock";
        if (stockActual <= stockMinimo * 0.5m) return "StockCritico";
        if (stockActual <= stockMinimo) return "BajoStock";
        if (stockActual > stockMaximo) return "Sobrestock";
        return "Optimo";
    }

    private decimal CalcularConsumoPromedioDiario(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var dias = (fechaFin - fechaInicio).TotalDays;
        if (dias <= 0) return 0;

        var consumoTotal = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Salida)
            .Sum(m => m.Cantidad);

        return consumoTotal / (decimal)dias;
    }

    private int CalcularDiasStockRestante(decimal stockActual, IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var consumoPromedio = CalcularConsumoPromedioDiario(movimientos, fechaInicio, fechaFin);
        if (consumoPromedio <= 0) return int.MaxValue;

        return (int)(stockActual / consumoPromedio);
    }

    private DateTime? CalcularFechaAgotamiento(decimal stockActual, IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var diasRestantes = CalcularDiasStockRestante(stockActual, movimientos, fechaInicio, fechaFin);
        if (diasRestantes == int.MaxValue) return null;

        return DateTime.Today.AddDays(diasRestantes);
    }

    private decimal CalcularCantidadRecomendada(decimal stockActual, decimal stockMinimo, decimal stockMaximo)
    {
        var deficit = stockMaximo - stockActual;
        return deficit > 0 ? deficit : 0;
    }

    private decimal CalcularTasaRotacion(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var salidas = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Salida)
            .Sum(m => m.Cantidad);

        var dias = (fechaFin - fechaInicio).TotalDays;
        return dias > 0 ? salidas / (decimal)dias : 0;
    }

    private decimal CalcularPorcentajeDesperdicios(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var totalSalidas = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Salida)
            .Sum(m => m.Cantidad);

        var desperdicios = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ajuste && m.Cantidad < 0)
            .Sum(m => Math.Abs(m.Cantidad));

        return totalSalidas > 0 ? (desperdicios / totalSalidas) * 100 : 0;
    }

    private string DeterminarTendenciaConsumo(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        // Implementación simplificada
        return "Estable";
    }

    private List<string> GenerarAlertasEspecificas(Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente)
    {
        var alertas = new List<string>();
        
        if (ingrediente.Stock <= ingrediente.StockMinimo * 0.5m)
            alertas.Add("Stock crítico");
        else if (ingrediente.Stock <= ingrediente.StockMinimo)
            alertas.Add("Stock bajo");
        
        return alertas;
    }

    private string DeterminarPrioridadReposicion(Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente)
    {
        if (ingrediente.Stock <= ingrediente.StockMinimo * 0.5m) return "Alta";
        if (ingrediente.Stock <= ingrediente.StockMinimo) return "Media";
        return "Baja";
    }

    private decimal CalcularPorcentajeStockOptimo(decimal stockActual, decimal stockMinimo, decimal stockMaximo)
    {
        if (stockMaximo <= stockMinimo) return 0;
        var rangoOptimo = stockMaximo - stockMinimo;
        var stockEnRango = stockActual - stockMinimo;
        return rangoOptimo > 0 ? (stockEnRango / rangoOptimo) * 100 : 0;
    }

    private decimal CalcularRotacionIngrediente(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return CalcularTasaRotacion(movimientos, fechaInicio, fechaFin);
    }

    private string GenerarSugerenciaAccion(Domain.Inventario.Ingredientes.Entities.Ingrediente ingrediente)
    {
        if (ingrediente.Stock <= ingrediente.StockMinimo * 0.5m)
            return "Reabastecer urgentemente";
        if (ingrediente.Stock <= ingrediente.StockMinimo)
            return "Planificar reabastecimiento";
        return "Stock en niveles normales";
    }

    private decimal CalcularTasaRotacionCategoria(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return CalcularTasaRotacion(movimientos, fechaInicio, fechaFin);
    }

    private string DeterminarEstadoCategoria(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var criticos = ingredientes.Count(i => i.Stock <= i.StockMinimo * 0.5m);
        var bajos = ingredientes.Count(i => i.Stock <= i.StockMinimo && i.Stock > i.StockMinimo * 0.5m);
        
        if (criticos > 0) return "Crítico";
        if (bajos > 0) return "Atención";
        return "Óptimo";
    }

    private List<string> GenerarRecomendacionesCategoria(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var recomendaciones = new List<string>();
        var criticos = ingredientes.Count(i => i.Stock <= i.StockMinimo * 0.5m);
        
        if (criticos > 0)
            recomendaciones.Add($"Reabastecer {criticos} ingredientes críticos");
        
        return recomendaciones;
    }

    private decimal CalcularRotacionPromedio(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return CalcularTasaRotacion(movimientos, fechaInicio, fechaFin);
    }

    private string DeterminarTendenciaCategoria(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return "Estable";
    }

    private List<AlertaInventarioDto> GenerarAlertasInventario(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var alertas = new List<AlertaInventarioDto>();

        foreach (var ingrediente in ingredientes.Where(i => i.Stock <= i.StockMinimo))
        {
            var esCritico = ingrediente.Stock <= ingrediente.StockMinimo * 0.5m;
            alertas.Add(new AlertaInventarioDto
            {
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                CodigoIngrediente = ingrediente.Codigo,
                Categoria = "Sin categoría", // TODO: Usar categoría real cuando esté disponible
                StockActual = ingrediente.Stock,
                StockMinimo = ingrediente.StockMinimo,
                TipoAlerta = esCritico ? "Crítico" : "Bajo",
                Severidad = esCritico ? "Alta" : "Media",
                Mensaje = $"El ingrediente {ingrediente.Nombre} tiene stock bajo (Actual: {ingrediente.Stock}, Mínimo: {ingrediente.StockMinimo})",
                FechaAlerta = _dateTimeService.Now,
                CantidadRecomendada = ingrediente.StockMinimo - ingrediente.Stock
            });
        }

        return alertas;
    }

    private List<RecomendacionCompraDto> GenerarRecomendacionesCompra(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var recomendaciones = new List<RecomendacionCompraDto>();

        foreach (var ingrediente in ingredientes.Where(i => i.Stock <= i.StockMinimo))
        {
            var cantidadRecomendada = CalcularCantidadRecomendada(ingrediente.Stock, ingrediente.StockMinimo, ingrediente.StockMinimo * 2);

            recomendaciones.Add(new RecomendacionCompraDto
            {
                IngredienteId = ingrediente.Id,
                NombreIngrediente = ingrediente.Nombre,
                CantidadRecomendada = cantidadRecomendada,
                UnidadMedida = ingrediente.UnidadMedida.ToString(),
                CostoEstimado = cantidadRecomendada * ingrediente.CostoPromedio,
                PrioridadCompra = ingrediente.Stock <= ingrediente.StockMinimo * 0.5m ? "Alta" : "Media",
                FechaRecomendadaPedido = DateTime.Today.AddDays(ingrediente.Stock <= ingrediente.StockMinimo * 0.5m ? 0 : 3),
                Justificacion = $"Stock actual ({ingrediente.Stock}) está por debajo del mínimo ({ingrediente.StockMinimo})",
                ProveedoresRecomendados = new List<string> { "Proveedor Principal" },
                AhorroEstimado = 0, // Se calcularía basado en precios históricos
                ImpactoSinCompra = "Interrupción en producción"
            });
        }

        return recomendaciones;
    }

    private decimal CalcularEficienciaGeneral(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var totalIngredientes = ingredientes.Count();
        if (totalIngredientes == 0) return 0;

        var ingredientesOptimos = ingredientes.Count(i => i.Stock > i.StockMinimo && i.Stock <= i.StockMinimo * 2);
        return (decimal)ingredientesOptimos / totalIngredientes * 100;
    }

    private decimal CalcularTasaRotacionGlobal(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return CalcularTasaRotacion(movimientos, fechaInicio, fechaFin);
    }

    private decimal CalcularPorcentajeStockOptimoGlobal(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        return CalcularEficienciaGeneral(ingredientes);
    }

    private string DeterminarClasificacionEficiencia(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var eficiencia = CalcularEficienciaGeneral(ingredientes);
        
        if (eficiencia >= 90) return "Excelente";
        if (eficiencia >= 75) return "Buena";
        if (eficiencia >= 60) return "Regular";
        return "Deficiente";
    }

    private List<MetricaIndividualDto> GenerarMetricasDetalladas(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        return new List<MetricaIndividualDto>
        {
            new MetricaIndividualDto
            {
                NombreMetrica = "Stock Óptimo",
                ValorActual = CalcularEficienciaGeneral(ingredientes),
                ValorObjetivo = 90,
                PorcentajeCumplimiento = CalcularEficienciaGeneral(ingredientes) / 90 * 100,
                Estado = DeterminarClasificacionEficiencia(ingredientes),
                Descripcion = "Porcentaje de ingredientes con stock en niveles óptimos"
            }
        };
    }

    private List<string> GenerarRecomendacionesMejora(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var recomendaciones = new List<string>();
        var criticos = ingredientes.Count(i => i.Stock <= i.StockMinimo * 0.5m);
        
        if (criticos > 0)
            recomendaciones.Add($"Implementar alertas automáticas para {criticos} ingredientes críticos");
        
        return recomendaciones;
    }

    private List<string> GenerarFiltrosAplicados(ObtenerAnalisisInventarioQuery request)
    {
        var filtros = new List<string>();
        
        if (request.SoloAlertaStock)
            filtros.Add("Solo con alertas de stock");
        if (request.SoloCriticos)
            filtros.Add("Solo críticos");
        
        return filtros;
    }

    private decimal CalcularPorcentajeDesperdiciosGlobal(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        var totalSalidas = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Salida)
            .Sum(m => m.Cantidad);

        var desperdicios = movimientos
            .Where(m => m.TipoMovimiento == Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ajuste && m.Cantidad < 0)
            .Sum(m => Math.Abs(m.Cantidad));

        return totalSalidas > 0 ? (desperdicios / totalSalidas) * 100 : 0;
    }

    private string DeterminarEstadoGeneralInventario(IEnumerable<Domain.Inventario.Ingredientes.Entities.Ingrediente> ingredientes)
    {
        var criticos = ingredientes.Count(i => i.Stock <= i.StockMinimo * 0.5m);
        var bajos = ingredientes.Count(i => i.Stock <= i.StockMinimo && i.Stock > i.StockMinimo * 0.5m);
        
        if (criticos > 0) return "Crítico";
        if (bajos > 0) return "Atención";
        return "Óptimo";
    }

    private string DeterminarTendenciaGeneral(IEnumerable<Domain.Inventario.Ingredientes.Movimientos.Entities.MovimientoInventario> movimientos, DateTime fechaInicio, DateTime fechaFin)
    {
        return "Estable";
    }
} 