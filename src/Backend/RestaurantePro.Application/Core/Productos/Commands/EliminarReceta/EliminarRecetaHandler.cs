using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Core.Productos.Commands.EliminarReceta;

/// <summary>
/// Handler para eliminar una receta
/// </summary>
public class EliminarRecetaHandler : IRequestHandler<EliminarRecetaCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EliminarRecetaHandler> _logger;

    public EliminarRecetaHandler(
        IApplicationDbContext context,
        ILogger<EliminarRecetaHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        EliminarRecetaCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🗑️ Eliminando receta: {Id}", request.Id);

            // 1. Obtener la receta
            var receta = await _context.Recetas
                .FirstOrDefaultAsync(r => r.Id == request.Id && !r.RecetaEliminada, cancellationToken);

            if (receta == null)
            {
                _logger.LogWarning("⚠️ Receta no encontrada: {Id}", request.Id);
                return Result.Failure<bool>("Receta no encontrada");
            }

            // 2. Marcar como eliminada (soft delete)
            receta.MarkAsDeleted();
            _context.Recetas.Update(receta);

            // 3. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Receta eliminada exitosamente: {Id}", request.Id);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al eliminar receta: {Id}", request.Id);
            return Result.Failure<bool>("Error al eliminar la receta");
        }
    }
} 