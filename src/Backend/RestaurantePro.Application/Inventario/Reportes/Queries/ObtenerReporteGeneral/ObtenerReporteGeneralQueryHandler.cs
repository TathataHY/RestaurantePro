using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerReporteGeneral;

/// <summary>
/// Handler para obtener el reporte general de inventario
/// </summary>
public class ObtenerReporteGeneralQueryHandler : IRequestHandler<ObtenerReporteGeneralQuery, Result<ReporteGeneralInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ObtenerReporteGeneralQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ReporteGeneralInventarioDto>> Handle(
        ObtenerReporteGeneralQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Construir query base
            var query = _context.Ingredientes.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(request.Categoria))
            {
                // Nota: El modelo Ingrediente no tiene propiedad Categoria, se omite este filtro
                // TODO: Implementar filtro por categoría cuando esté disponible en el modelo
            }

            // Nota: Los filtros StockBajo y StockCritico no están disponibles en el query
            // TODO: Implementar estos filtros cuando estén disponibles

            // Obtener ingredientes
            var ingredientes = await query
                .Select(i => new IngredienteReporteDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Codigo = i.Codigo,
                    Categoria = "Sin categoría", // TODO: Usar categoría real cuando esté disponible
                    StockActual = i.Stock,
                    StockMinimo = i.StockMinimo,
                    PrecioUnitario = i.CostoPromedio, // Usar CostoPromedio en lugar de PrecioUnitario
                    ValorTotal = i.Stock * i.CostoPromedio,
                    EstadoStock = GetEstadoStock(i.Stock, i.StockMinimo),
                    UltimaActualizacion = DateTime.Now // TODO: Usar fecha real cuando esté disponible
                })
                .ToListAsync(cancellationToken);

            // Calcular totales
            var totalIngredientes = ingredientes.Count;
            var valorTotalInventario = ingredientes.Sum(i => i.ValorTotal);
            var ingredientesStockBajo = ingredientes.Count(i => i.EstadoStock == "Bajo");
            var ingredientesStockCritico = ingredientes.Count(i => i.EstadoStock == "Crítico");

            // Generar resumen por categorías
            var resumenPorCategorias = ingredientes
                .GroupBy(i => i.Categoria)
                .Select(g => new CategoriaResumenDto
                {
                    Categoria = g.Key,
                    CantidadIngredientes = g.Count(),
                    ValorTotal = g.Sum(i => i.ValorTotal),
                    StockBajo = g.Count(i => i.EstadoStock == "Bajo"),
                    StockCritico = g.Count(i => i.EstadoStock == "Crítico")
                })
                .ToList();

            // Construir respuesta
            var reporte = new ReporteGeneralInventarioDto
            {
                FechaGeneracion = _dateTimeService.Now,
                TotalIngredientes = totalIngredientes,
                ValorTotalInventario = valorTotalInventario,
                IngredientesStockBajo = ingredientesStockBajo,
                IngredientesStockCritico = ingredientesStockCritico,
                Ingredientes = ingredientes,
                ResumenPorCategorias = resumenPorCategorias
            };

            return Result.Success(reporte);
        }
        catch (Exception ex)
        {
            return Result.Failure<ReporteGeneralInventarioDto>("Error al generar reporte general de inventario");
        }
    }

    private static string GetEstadoStock(decimal stock, decimal stockMinimo)
    {
        if (stock <= stockMinimo * 0.5m)
            return "Crítico";
        if (stock <= stockMinimo)
            return "Bajo";
        return "Normal";
    }
} 