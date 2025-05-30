using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;

namespace RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;

/// <summary>
/// Handler para LiberarMesaCommand
/// </summary>
public class LiberarMesaHandler : IRequestHandler<LiberarMesaCommand, Result<Unit>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<LiberarMesaHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public LiberarMesaHandler(
        IMesaRepository mesaRepository,
        ILogger<LiberarMesaHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(LiberarMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🆓 Iniciando liberación de mesa {MesaId} por usuario {UserId}", 
                request.MesaId, _currentUser.UserId);

            // Obtener la mesa
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId);
            if (mesa == null)
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} no encontrada", request.MesaId);
                return Result.Failure<Unit>("Mesa no encontrada");
            }

            // Verificar estado actual de la mesa
            _logger.LogDebug("Mesa {MesaId} encontrada. Estado actual: {Estado}, Capacidad: {Capacidad}, Ubicación: {Ubicacion}", 
                mesa.Id, mesa.Estado, mesa.Capacidad, mesa.Ubicacion);

            // Marcar como disponible usando la lógica de dominio
            mesa.MarcarComoDisponible();

            // Actualizar en el repositorio
            await _mesaRepository.ActualizarAsync(mesa);
            await _mesaRepository.GuardarCambiosAsync();

            _logger.LogInformation("✅ Mesa {NumeroMesa} (ID: {MesaId}) liberada correctamente y marcada como disponible", 
                mesa.Numero, mesa.Id);

            if (!string.IsNullOrEmpty(request.Observaciones))
            {
                _logger.LogInformation("📝 Observaciones de liberación: {Observaciones}", request.Observaciones);
            }

            return Result.Success(Unit.Value);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error de negocio al liberar mesa {MesaId}: {Error}", 
                request.MesaId, ex.Message);
            return Result.Failure<Unit>($"Error al liberar mesa: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error interno al liberar mesa {MesaId}", request.MesaId);
            return Result.Failure<Unit>("Error interno del servidor al liberar la mesa");
        }
    }
} 