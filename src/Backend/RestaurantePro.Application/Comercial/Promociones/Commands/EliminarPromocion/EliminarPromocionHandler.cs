using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.Commands.EliminarPromocion;

/// <summary>
/// Handler para eliminar una promoción
/// </summary>
public class EliminarPromocionHandler : IRequestHandler<EliminarPromocionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EliminarPromocionHandler> _logger;

    public EliminarPromocionHandler(
        IApplicationDbContext context,
        ILogger<EliminarPromocionHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(EliminarPromocionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🗑️ Eliminando promoción: {Id}", request.Id);

            var promocion = await _context.Promociones
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (promocion == null)
            {
                _logger.LogWarning("⚠️ Promoción no encontrada: {Id}", request.Id);
                return Result.Failure<bool>("La promoción no existe");
            }

            // Validar que la promoción no esté ya cancelada
            if (promocion.Estado == EstadoPromocion.Cancelada)
            {
                return Result.Failure<bool>("La promoción ya está cancelada");
            }

            // Validar que la promoción no esté finalizada
            if (promocion.Estado == EstadoPromocion.Finalizada)
            {
                return Result.Failure<bool>("No se puede cancelar una promoción finalizada");
            }

            // Cancelar la promoción
            promocion.Cancelar(request.Motivo);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Promoción eliminada exitosamente: {Id}", request.Id);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando promoción: {Id}", request.Id);
            return Result.Failure<bool>($"Error eliminando promoción: {ex.Message}");
        }
    }
} 