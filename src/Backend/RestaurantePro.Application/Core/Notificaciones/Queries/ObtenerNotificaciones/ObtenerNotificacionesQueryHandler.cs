namespace RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificaciones;

/// <summary>
/// Handler para obtener notificaciones
/// </summary>
public class ObtenerNotificacionesQueryHandler : IRequestHandler<ObtenerNotificacionesQuery, Result<List<NotificacionDto>>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<ObtenerNotificacionesQueryHandler> _logger;

    public ObtenerNotificacionesQueryHandler(
        INotificacionRepository notificacionRepository,
        ILogger<ObtenerNotificacionesQueryHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<List<NotificacionDto>>> Handle(ObtenerNotificacionesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var notificaciones = await _notificacionRepository.ObtenerPorDestinatarioAsync(request.UsuarioId, cancellationToken);

            if (request.SoloNoLeidas)
            {
                notificaciones = notificaciones.Where(n => !n.EstaLeida).ToList();
            }

            var notificacionesDto = notificaciones.Select(n => new NotificacionDto
            {
                Id = n.Id,
                Titulo = n.Titulo,
                Mensaje = n.Mensaje,
                Tipo = n.Tipo.ToString(),
                FechaCreacion = n.FechaCreacion,
                FechaLectura = n.FechaLectura,
                EstaLeida = n.EstaLeida,
                EntidadRelacionadaId = n.EntidadRelacionadaId
            }).ToList();

            _logger.LogInformation("Obtenidas {Cantidad} notificaciones para el usuario: {UsuarioId}", 
                notificacionesDto.Count, request.UsuarioId);

            return Result.Success(notificacionesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones para el usuario: {UsuarioId}", request.UsuarioId);
            return Result.Failure<List<NotificacionDto>>("Error interno al obtener las notificaciones");
        }
    }
} 