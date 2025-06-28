using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerValorTotalInventario;

public class ObtenerValorTotalInventarioQueryHandler : IRequestHandler<ObtenerValorTotalInventarioQuery, Result<ValorTotalInventarioDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ObtenerValorTotalInventarioQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ValorTotalInventarioDto>> Handle(
        ObtenerValorTotalInventarioQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ingredientes = await _context.Ingredientes
                .Where(i => i.EstaActivo)
                .ToListAsync(cancellationToken);

            var valorTotal = ingredientes.Sum(i => i.Stock * i.CostoPromedio);
            var totalIngredientes = ingredientes.Count;
            var promedioValorPorIngrediente = totalIngredientes > 0 ? valorTotal / totalIngredientes : 0;

            var valoresPorCategoria = ingredientes
                .GroupBy(i => "Sin categoría")
                .Select(g => new ValorPorCategoriaDto
                {
                    Categoria = g.Key,
                    ValorTotal = g.Sum(i => i.Stock * i.CostoPromedio),
                    CantidadIngredientes = g.Count(),
                    PorcentajeDelTotal = valorTotal > 0 ? (g.Sum(i => i.Stock * i.CostoPromedio) / valorTotal) * 100 : 0
                })
                .ToList();

            var resultado = new ValorTotalInventarioDto
            {
                ValorTotal = valorTotal,
                Moneda = "USD",
                FechaCalculo = _dateTimeService.Now,
                ValoresPorCategoria = valoresPorCategoria,
                TotalIngredientes = totalIngredientes,
                PromedioValorPorIngrediente = promedioValorPorIngrediente
            };

            return Result.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result.Failure<ValorTotalInventarioDto>($"Error al calcular valor total del inventario: {ex.Message}");
        }
    }
} 