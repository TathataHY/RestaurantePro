using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.EliminarComanda;

/// <summary>
/// Handler para eliminar una comanda (soft delete)
/// </summary>
public class EliminarComandaCommandHandler : IRequestHandler<EliminarComandaCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EliminarComandaCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTime;

    public EliminarComandaCommandHandler(
        IApplicationDbContext context,
        ILogger<EliminarComandaCommandHandler> logger,
        ICurrentUserService currentUserService,
        IDateTimeService dateTime)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<bool>> Handle(EliminarComandaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🗑️ Eliminando comanda con ID: {ComandaId}", request.ComandaId);

            // Buscar la comanda
            var comanda = await _context.Comandas
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.ComandaId, cancellationToken);

            if (comanda == null)
            {
                _logger.LogWarning("⚠️ Comanda no encontrada con ID: {ComandaId}", request.ComandaId);
                return Result.Failure<bool>($"Comanda con ID {request.ComandaId} no encontrada");
            }

            // Validar que la comanda se puede eliminar
            var validacionResult = ValidarEliminacion(comanda);
            if (validacionResult.IsFailure())
            {
                return Result.Failure<bool>(validacionResult.Error ?? "Error validando eliminación");
            }

            // Realizar soft delete
            comanda.Eliminar(_currentUserService.UserId, _dateTime.Now);

            // Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Comanda eliminada exitosamente - ID: {ComandaId}, Número: {NumeroComanda}",
                comanda.Id, comanda.NumeroComanda);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error eliminando comanda {ComandaId}: {ErrorMessage}", 
                request.ComandaId, ex.Message);
            return Result.Failure<bool>($"Error eliminando comanda: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida si la comanda se puede eliminar
    /// </summary>
    private Result ValidarEliminacion(Domain.Operaciones.Comandas.Entities.Comanda comanda)
    {
        // No se puede eliminar comandas que ya han sido finalizadas
        if (comanda.Estado == EstadoComanda.Finalizada)
        {
            return Result.Failure("No se puede eliminar una comanda que ya ha sido finalizada");
        }

        // No se puede eliminar comandas que ya han sido canceladas
        if (comanda.Estado == EstadoComanda.Cancelada)
        {
            return Result.Failure("No se puede eliminar una comanda que ya ha sido cancelada");
        }

        // No se puede eliminar comandas con items en preparación
        if (comanda.Items.Any(i => i.Estado == Domain.Operaciones.Comandas.Enums.EstadoItemComanda.EnPreparacion))
        {
            return Result.Failure("No se puede eliminar una comanda con items en preparación");
        }

        return Result.Success();
    }
} 