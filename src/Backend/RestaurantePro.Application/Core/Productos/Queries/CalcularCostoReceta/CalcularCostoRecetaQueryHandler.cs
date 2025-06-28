using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Productos.Services;

namespace RestaurantePro.Application.Core.Productos.Queries.CalcularCostoReceta;

/// <summary>
/// Handler para calcular el costo total de ingredientes de una receta
/// </summary>
public class CalcularCostoRecetaQueryHandler : IRequestHandler<CalcularCostoRecetaQuery, Result<decimal>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalculoRecetaService _calculoRecetaService;
    private readonly ILogger<CalcularCostoRecetaQueryHandler> _logger;

    public CalcularCostoRecetaQueryHandler(
        IApplicationDbContext context,
        ICalculoRecetaService calculoRecetaService,
        ILogger<CalcularCostoRecetaQueryHandler> logger)
    {
        _context = context;
        _calculoRecetaService = calculoRecetaService;
        _logger = logger;
    }

    public async Task<Result<decimal>> Handle(
        CalcularCostoRecetaQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💰 Calculando costo de receta: {Id}, Porciones: {Porciones}", 
                request.Id, request.CantidadPorciones);

            // 1. Verificar que la receta existe
            var receta = await _context.Recetas
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (receta == null)
            {
                _logger.LogWarning("⚠️ Receta no encontrada: {Id}", request.Id);
                return Result.Failure<decimal>("Receta no encontrada");
            }

            var recetaEliminada = receta.RecetaEliminada;
            if (recetaEliminada)
            {
                _logger.LogWarning("⚠️ Receta eliminada: {Id}", request.Id);
                return Result.Failure<decimal>("Receta no encontrada");
            }

            // 2. Calcular costo usando el servicio de dominio
            var costoResult = await _calculoRecetaService.CalcularCostoRecetaAsync(receta.ProductoId, cancellationToken);

            if (!costoResult.Succeeded)
            {
                _logger.LogWarning("⚠️ Error al calcular costo: {Error}", costoResult.Error);
                return Result.Failure<decimal>(costoResult.Error);
            }

            var costoUnitario = costoResult.Value;

            // 3. Multiplicar por la cantidad de porciones
            var costoTotal = costoUnitario * request.CantidadPorciones;

            _logger.LogInformation("✅ Costo calculado: ${Costo} para {Porciones} porciones", 
                costoTotal, request.CantidadPorciones);

            return Result.Success(costoTotal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al calcular costo de receta: {Id}", request.Id);
            return Result.Failure<decimal>("Error al calcular el costo de la receta");
        }
    }
} 