using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerReporteValoracion;

public class ObtenerReporteValoracionQueryHandler : IRequestHandler<ObtenerReporteValoracionQuery, Result<ReporteValoracionDto>>
{
    private readonly IApplicationDbContext _context;

    public ObtenerReporteValoracionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReporteValoracionDto>> Handle(ObtenerReporteValoracionQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Ingredientes.AsQueryable();

        // Filtrar por fechas si se proporcionan
        if (request.FechaDesde.HasValue)
        {
            query = query.Where(i => i.FechaCreacion >= request.FechaDesde.Value);
        }

        if (request.FechaHasta.HasValue)
        {
            query = query.Where(i => i.FechaCreacion <= request.FechaHasta.Value);
        }

        // Filtrar ingredientes sin stock si no se incluyen
        if (!request.IncluirIngredientesSinStock)
        {
            query = query.Where(i => i.Stock > 0);
        }

        var ingredientes = await query.ToListAsync(cancellationToken);

        var reporte = new ReporteValoracionDto
        {
            FechaGeneracion = DateTime.UtcNow,
            TotalIngredientes = ingredientes.Count,
            ValorTotalInventario = ingredientes.Sum(i => i.Stock * i.CostoPromedio),
            IngredientesConStock = ingredientes.Count(i => i.Stock > 0),
            IngredientesSinStock = ingredientes.Count(i => i.Stock <= 0),
            IngredientesBajoStock = ingredientes.Count(i => i.Stock <= i.StockMinimo && i.Stock > 0),
            DetallePorIngrediente = ingredientes.Select(i => new ValoracionIngredienteDto
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Stock = i.Stock,
                PrecioUnitario = i.CostoPromedio,
                ValorTotal = i.Stock * i.CostoPromedio,
                UnidadMedida = i.UnidadMedida,
                StockMinimo = i.StockMinimo,
                StockMaximo = 0,
                EstadoStock = i.Stock <= 0 ? "Sin Stock" : 
                             i.Stock <= i.StockMinimo ? "Bajo Stock" : "Normal"
            }).ToList()
        };

        return Result.Success(reporte);
    }
} 