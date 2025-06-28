using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Inventario.Reportes.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAlertasInventario;

/// <summary>
/// Handler para obtener alertas de inventario
/// </summary>
public class ObtenerAlertasInventarioQueryHandler : IRequestHandler<ObtenerAlertasInventarioQuery, Result<List<AlertaInventarioDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ObtenerAlertasInventarioQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<List<AlertaInventarioDto>>> Handle(
        ObtenerAlertasInventarioQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var alertas = new List<AlertaInventarioDto>();

            // Obtener ingredientes con stock bajo
            var ingredientesBajoStock = await _context.Ingredientes
                .Where(i => i.Stock <= i.StockMinimo)
                .ToListAsync(cancellationToken);

            foreach (var ingrediente in ingredientesBajoStock)
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

            return Result.Success(alertas);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<AlertaInventarioDto>>($"Error al obtener alertas de inventario: {ex.Message}");
        }
    }
} 