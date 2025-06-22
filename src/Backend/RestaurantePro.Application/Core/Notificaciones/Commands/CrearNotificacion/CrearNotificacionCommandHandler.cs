namespace RestaurantePro.Application.Core.Notificaciones.Commands.CrearNotificacion;

/// <summary>
/// Handler para el comando de crear notificación
/// </summary>
public class CrearNotificacionCommandHandler : IRequestHandler<CrearNotificacionCommand, Result<NotificacionDto>>
{
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<CrearNotificacionCommandHandler> _logger;

    public CrearNotificacionCommandHandler(
        INotificacionRepository notificacionRepository,
        ILogger<CrearNotificacionCommandHandler> logger)
    {
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task<Result<NotificacionDto>> Handle(CrearNotificacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Parsear el tipo de notificación
            if (!Enum.TryParse<TipoNotificacion>(request.Tipo, true, out var tipoNotificacion))
            {
                tipoNotificacion = TipoNotificacion.Informativa; // Valor por defecto
            }

            // Crear la notificación usando el método estático de la entidad
            var notificacion = Notificacion.Crear(
                request.Titulo,
                request.Mensaje,
                tipoNotificacion,
                request.DestinatarioId,
                request.EntidadRelacionadaId);

            // Guardar en el repositorio
            await _notificacionRepository.AgregarAsync(notificacion, cancellationToken);
            await _notificacionRepository.GuardarCambiosAsync(cancellationToken);

            // Mapear a DTO
            var notificacionDto = new NotificacionDto
            {
                Id = notificacion.Id,
                Titulo = notificacion.Titulo,
                Mensaje = notificacion.Mensaje,
                Tipo = notificacion.Tipo.ToString(),
                FechaCreacion = notificacion.FechaCreacion,
                FechaLectura = notificacion.FechaLectura,
                EstaLeida = notificacion.EstaLeida,
                EntidadRelacionadaId = notificacion.EntidadRelacionadaId
            };

            _logger.LogInformation("Notificación creada exitosamente: {NotificacionId} para usuario: {DestinatarioId}", 
                notificacion.Id, request.DestinatarioId);

            return Result.Success(notificacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación para usuario: {DestinatarioId}", request.DestinatarioId);
            return Result.Failure<NotificacionDto>("Error interno al crear la notificación");
        }
    }
} 