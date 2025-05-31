namespace RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;

/// <summary>
/// Handler para CambiarEstadoMesaCommand
/// </summary>
public class CambiarEstadoMesaHandler : IRequestHandler<CambiarEstadoMesaCommand, Result<Unit>>
{
    private readonly IMesaRepository _mesaRepository;
    private readonly ILogger<CambiarEstadoMesaHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public CambiarEstadoMesaHandler(
        IMesaRepository mesaRepository,
        ILogger<CambiarEstadoMesaHandler> logger,
        ICurrentUserService currentUser)
    {
        _mesaRepository = mesaRepository;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(CambiarEstadoMesaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔄 Iniciando cambio de estado de mesa {MesaId} a {NuevoEstado} por usuario {UserId}", 
                request.MesaId, request.NuevoEstado, _currentUser.UserId);

            // Obtener la mesa
            var mesa = await _mesaRepository.ObtenerPorIdAsync(request.MesaId);
            if (mesa == null)
            {
                _logger.LogWarning("⚠️ Mesa {MesaId} no encontrada", request.MesaId);
                return Result.Failure<Unit>("Mesa no encontrada");
            }

            // Verificar estado actual de la mesa
            _logger.LogDebug("Mesa {MesaId} encontrada. Estado actual: {EstadoActual} → Nuevo estado: {NuevoEstado}", 
                mesa.Id, mesa.Estado, request.NuevoEstado);

            // Aplicar el cambio de estado según el estado solicitado
            switch (request.NuevoEstado)
            {
                case EstadoMesa.Disponible:
                    mesa.MarcarComoDisponible();
                    break;

                case EstadoMesa.Ocupada:
                    mesa.MarcarComoOcupada();
                    break;

                case EstadoMesa.Reservada:
                    mesa.MarcarComoReservada();
                    break;

                case EstadoMesa.FueraDeServicio:
                    mesa.MarcarComoFueraDeServicio(request.Motivo!);
                    break;

                default:
                    _logger.LogWarning("⚠️ Estado {NuevoEstado} no válido para mesa {MesaId}", 
                        request.NuevoEstado, request.MesaId);
                    return Result.Failure<Unit>($"Estado {request.NuevoEstado} no es válido");
            }

            // Actualizar en el repositorio
            await _mesaRepository.ActualizarAsync(mesa);
            await _mesaRepository.GuardarCambiosAsync();

            _logger.LogInformation("✅ Mesa {NumeroMesa} (ID: {MesaId}) cambió de estado correctamente a {NuevoEstado}", 
                mesa.Numero, mesa.Id, request.NuevoEstado);

            if (!string.IsNullOrEmpty(request.Motivo))
            {
                _logger.LogInformation("🔍 Motivo del cambio: {Motivo}", request.Motivo);
            }

            if (!string.IsNullOrEmpty(request.Observaciones))
            {
                _logger.LogInformation("📝 Observaciones: {Observaciones}", request.Observaciones);
            }

            return Result.Success(Unit.Value);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error de negocio al cambiar estado de mesa {MesaId}: {Error}", 
                request.MesaId, ex.Message);
            return Result.Failure<Unit>($"Error al cambiar estado de mesa: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error interno al cambiar estado de mesa {MesaId}", request.MesaId);
            return Result.Failure<Unit>("Error interno del servidor al cambiar el estado de la mesa");
        }
    }
}