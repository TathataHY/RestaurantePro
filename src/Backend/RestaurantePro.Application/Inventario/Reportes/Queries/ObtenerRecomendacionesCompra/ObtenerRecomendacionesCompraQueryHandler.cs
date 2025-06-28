using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Inventario.Reportes.DTOs;

namespace RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerRecomendacionesCompra;

public class ObtenerRecomendacionesCompraQueryHandler : IRequestHandler<ObtenerRecomendacionesCompraQuery, Result<List<RecomendacionCompraDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public ObtenerRecomendacionesCompraQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<List<RecomendacionCompraDto>>> Handle(
        ObtenerRecomendacionesCompraQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var recomendaciones = new List<RecomendacionCompraDto>();

            // Obtener ingredientes que necesitan reabastecimiento
            var ingredientesNecesitanCompra = await _context.Ingredientes
                .Where(i => i.Stock <= i.StockMinimo)
                .ToListAsync(cancellationToken);

            foreach (var ingrediente in ingredientesNecesitanCompra)
            {
                var cantidadRecomendada = CalcularCantidadRecomendada(ingrediente.Stock, ingrediente.StockMinimo);
                var esUrgente = ingrediente.Stock <= ingrediente.StockMinimo * 0.5m;

                recomendaciones.Add(new RecomendacionCompraDto
                {
                    IngredienteId = ingrediente.Id,
                    NombreIngrediente = ingrediente.Nombre,
                    CantidadRecomendada = cantidadRecomendada,
                    UnidadMedida = ingrediente.UnidadMedida.ToString(),
                    CostoEstimado = cantidadRecomendada * ingrediente.CostoPromedio,
                    PrioridadCompra = esUrgente ? "Alta" : "Media",
                    FechaRecomendadaPedido = DateTime.Today.AddDays(esUrgente ? 0 : 3),
                    Justificacion = $"Stock actual ({ingrediente.Stock}) está por debajo del mínimo ({ingrediente.StockMinimo})",
                    ProveedoresRecomendados = new List<string> { "Proveedor Principal" },
                    AhorroEstimado = 0, // Se calcularía basado en precios históricos
                    ImpactoSinCompra = "Interrupción en producción"
                });
            }

            // Ordenar por prioridad
            var resultadoOrdenado = recomendaciones.OrderByDescending(r => r.PrioridadCompra == "Alta").ToList();
            return Result.Success(resultadoOrdenado);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<RecomendacionCompraDto>>($"Error al obtener recomendaciones de compra: {ex.Message}");
        }
    }

    private decimal CalcularCantidadRecomendada(decimal stockActual, decimal stockMinimo)
    {
        // Calcular cantidad para llegar al stock óptimo (2x el mínimo)
        var stockOptimo = stockMinimo * 2;
        var deficit = stockOptimo - stockActual;
        return deficit > 0 ? deficit : 0;
    }
} 