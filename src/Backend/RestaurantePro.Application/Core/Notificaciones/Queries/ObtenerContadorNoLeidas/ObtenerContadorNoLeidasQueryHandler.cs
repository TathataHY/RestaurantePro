namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerContadorNoLeidas;

/// <summary>
/// Handler para obtener el contador de notificaciones no leídas
/// </summary>
public class ObtenerContadorNoLeidasQueryHandler : IRequestHandler<ObtenerContadorNoLeidasQuery, Result<int>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<ObtenerContadorNoLeidasQueryHandler> _logger;

    public ObtenerContadorNoLeidasQueryHandler(
        INotificacionRepository notificacionRepository,
        ILogger<ObtenerContadorNoLeidasQueryHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(ObtenerContadorNoLeidasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notificaciones = await _notificacionRepository.ObtenerPorDestinatarioAsync(request.UsuarioId, cancellationToken);
            var contador = notificaciones.Count(n => !n.EstaLeida);

            _logger.LogInformation("Contador de notificaciones no leídas para usuario {UsuarioId}: {Contador}", 
                request.UsuarioId, contador);

            return Result.Success(contador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener contador de notificaciones no leídas para usuario: {UsuarioId}", 
                request.UsuarioId);
            return Result.Failure<int>("Error interno al obtener el contador de notificaciones no leídas");
        }
    }
} 