using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Core.Productos.DTOs;
using RestaurantePro.Domain.Core.Productos.Services;

namespace RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadReceta;

/// <summary>
/// Handler para verificar disponibilidad de ingredientes para una receta
/// </summary>
public class VerificarDisponibilidadRecetaQueryHandler : IRequestHandler<VerificarDisponibilidadRecetaQuery, Result<DisponibilidadRecetaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalculoRecetaService _calculoRecetaService;
    private readonly ILogger<VerificarDisponibilidadRecetaQueryHandler> _logger;

    public VerificarDisponibilidadRecetaQueryHandler(
        IApplicationDbContext context,
        ICalculoRecetaService calculoRecetaService,
        ILogger<VerificarDisponibilidadRecetaQueryHandler> logger)
    {
        _context = context;
        _calculoRecetaService = calculoRecetaService;
        _logger = logger;
    }

    public async Task<Result<DisponibilidadRecetaDto>> Handle(
        VerificarDisponibilidadRecetaQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✅ Verificando disponibilidad de receta: {Id}, Cantidad: {Cantidad}", 
                request.Id, request.Cantidad);

            // 1. Verificar que la receta existe
            var receta = await _context.Recetas
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (receta == null)
            {
                _logger.LogWarning("⚠️ Receta no encontrada: {Id}", request.Id);
                return Result.Failure<DisponibilidadRecetaDto>("Receta no encontrada");
            }

            var recetaEliminada = receta.RecetaEliminada;
            if (recetaEliminada)
            {
                _logger.LogWarning("⚠️ Receta eliminada: {Id}", request.Id);
                return Result.Failure<DisponibilidadRecetaDto>("Receta no encontrada");
            }

            // 2. Verificar disponibilidad usando el servicio de dominio
            var disponibilidadResult = await _calculoRecetaService.VerificarDisponibilidadIngredientesAsync(
                receta.ProductoId, request.Cantidad, cancellationToken);

            if (!disponibilidadResult.Succeeded)
            {
                _logger.LogWarning("⚠️ Error al verificar disponibilidad: {Error}", disponibilidadResult.Error);
                return Result.Failure<DisponibilidadRecetaDto>(disponibilidadResult.Error);
            }

            var estaDisponible = disponibilidadResult.Value;

            // 3. Construir respuesta detallada
            var disponibilidadDto = new DisponibilidadRecetaDto
            {
                EstaDisponible = estaDisponible,
                IngredientesFaltantes = new List<string>(), // Se puede expandir para mostrar ingredientes específicos
                MaximaPorcionesDisponibles = estaDisponible ? request.Cantidad : 0
            };

            // 4. Si no está disponible, calcular cuántas porciones se pueden hacer
            if (!estaDisponible)
            {
                // Aquí se podría implementar lógica más sofisticada para calcular porciones disponibles
                disponibilidadDto.MaximaPorcionesDisponibles = 0;
                disponibilidadDto.IngredientesFaltantes.Add("Algunos ingredientes no están disponibles en suficiente cantidad");
            }

            _logger.LogInformation("✅ Disponibilidad verificada: {Disponible} para {Cantidad} porciones", 
                estaDisponible, request.Cantidad);

            return Result.Success(disponibilidadDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al verificar disponibilidad de receta: {Id}", request.Id);
            return Result.Failure<DisponibilidadRecetaDto>("Error al verificar disponibilidad de la receta");
        }
    }
} 