namespace RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;

/// <summary>
/// Handler para AsignarMesaCommand
/// </summary>
public class AsignarMesaHandler : IRequestHandler<AsignarMesaCommand, Result<Unit>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<AsignarMesaHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public AsignarMesaHandler(
        IMesaRepository mesaRepository,
        ILogger<AsignarMesaHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(AsignarMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🪑 Iniciando asignación de mesa {MesaId} por usuario {UserId}", 
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

            // Marcar como ocupada usando la lógica de dominio
            mesa.MarcarComoOcupada();

            // Actualizar en el repositorio
            await _mesaRepository.ActualizarAsync(mesa);
            await _mesaRepository.GuardarCambiosAsync();

            _logger.LogInformation("✅ Mesa {NumeroMesa} (ID: {MesaId}) asignada correctamente", 
                mesa.Numero, mesa.Id);

            if (!string.IsNullOrEmpty(request.Observaciones))
            {
                _logger.LogInformation("📝 Observaciones de asignación: {Observaciones}", request.Observaciones);
            }

            return Result.Success(Unit.Value);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error de negocio al asignar mesa {MesaId}: {Error}", 
                request.MesaId, ex.Message);
            return Result.Failure<Unit>($"Error al asignar mesa: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error interno al asignar mesa {MesaId}", request.MesaId);
            return Result.Failure<Unit>("Error interno del servidor al asignar la mesa");
        }
    }
} 