namespace RestaurantePro.Application.Core.Notificaciones.Commands.MarcarTodasComoLeidas;

/// <summary>
/// Handler para marcar todas las notificaciones como leídas
/// </summary>
public class MarcarTodasNotificacionesComoLeidasCommandHandler : IRequestHandler<MarcarTodasNotificacionesComoLeidasCommand, Result<int>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<MarcarTodasNotificacionesComoLeidasCommandHandler> _logger;

    public MarcarTodasNotificacionesComoLeidasCommandHandler(
        INotificacionRepository notificacionRepository,
        ILogger<MarcarTodasNotificacionesComoLeidasCommandHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(MarcarTodasNotificacionesComoLeidasCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Obtener todas las notificaciones no leídas del usuario
            var notificaciones = await _notificacionRepository.ObtenerPorDestinatarioAsync(request.UsuarioId, cancellationToken);
            var notificacionesNoLeidas = notificaciones.Where(n => !n.EstaLeida).ToList();

            if (!notificacionesNoLeidas.Any())
            {
                _logger.LogInformation("No hay notificaciones no leídas para el usuario: {UsuarioId}", request.UsuarioId);
                return Result.Success(0);
            }

            // Marcar todas como leídas
            foreach (var notificacion in notificacionesNoLeidas)
            {
                notificacion.MarcarComoLeida();
            }

            // Guardar cambios
            await _notificacionRepository.GuardarCambiosAsync(cancellationToken);

            _logger.LogInformation("Marcadas {Cantidad} notificaciones como leídas para el usuario: {UsuarioId}", 
                notificacionesNoLeidas.Count, request.UsuarioId);

            return Result.Success(notificacionesNoLeidas.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificaciones como leídas para el usuario: {UsuarioId}", request.UsuarioId);
            return Result.Failure<int>("Error interno al marcar las notificaciones como leídas");
        }
    }
} 