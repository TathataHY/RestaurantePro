namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

/// <summary>
/// Handler para confirmar reservaciones
/// Gestiona el proceso de confirmación y notificaciones correspondientes
/// </summary>
public class ConfirmarReservacionHandler : IRequestHandler<ConfirmarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly INotificacionService _notificacionService;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfirmarReservacionHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public ConfirmarReservacionHandler(
        IReservacionRepository reservacionRepository,
        INotificacionService notificacionService,
        IMapper mapper,
        ILogger<ConfirmarReservacionHandler> logger,
        ICurrentUserService currentUser)
    {
        _reservacionRepository = reservacionRepository;
        _notificacionService = notificacionService;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservacionDto>> Handle(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando confirmación de reservación: ID={ReservacionId}, Código={CodigoReservacion}", 
            request.ReservacionId, request.CodigoReservacion);

        try
        {
            // 1. Buscar la reservación
            Reservacion? reservacion = null;
            
            if (request.ReservacionId != Guid.Empty)
            {
                reservacion = await _reservacionRepository.ObtenerPorIdAsync(request.ReservacionId);
            }
            else if (!string.IsNullOrEmpty(request.CodigoReservacion))
            {
                reservacion = await _reservacionRepository.ObtenerPorCodigoAsync(request.CodigoReservacion);
            }

            if (reservacion == null)
            {
                _logger.LogWarning("Reservación no encontrada: ID={ReservacionId}, Código={CodigoReservacion}", 
                    request.ReservacionId, request.CodigoReservacion);
                return Result<ReservacionDto>.Failure("La reservación especificada no existe");
            }

            // 2. Verificar que la reservación requiere confirmación
            if (!reservacion.RequiereConfirmacion)
            {
                _logger.LogWarning("Intento de confirmar reservación que no requiere confirmación: {CodigoReservacion}", 
                    reservacion.CodigoReservacion);
                return Result<ReservacionDto>.Failure("Esta reservación no requiere confirmación");
            }

            // 3. Verificar estado actual
            if (reservacion.EstadoConfirmacion == EstadoConfirmacion.Confirmada)
            {
                _logger.LogWarning("Reservación ya confirmada: {CodigoReservacion}", reservacion.CodigoReservacion);
                return Result<ReservacionDto>.Failure("La reservación ya ha sido confirmada");
            }

            if (reservacion.Estado == EstadoReservacion.Cancelada)
            {
                _logger.LogWarning("Intento de confirmar reservación cancelada: {CodigoReservacion}", reservacion.CodigoReservacion);
                return Result<ReservacionDto>.Failure("No se puede confirmar una reservación cancelada");
            }

            // 4. Verificar que no ha expirado el tiempo de confirmación
            var tiempoExpiracion = reservacion.FechaHoraReservacion.AddHours(-2); // 2 horas antes
            if (DateTime.UtcNow > tiempoExpiracion)
            {
                _logger.LogWarning("Tiempo de confirmación expirado para reservación: {CodigoReservacion}", reservacion.CodigoReservacion);
                return Result<ReservacionDto>.Failure("El tiempo para confirmar la reservación ha expirado");
            }

            // 5. Confirmar la reservación
            var resultadoConfirmacion = reservacion.Confirmar(
                request.MetodoConfirmacion,
                request.ConfirmadoPor ?? _currentUser.UserName ?? "Sistema",
                request.NotasConfirmacion,
                _currentUser.UserId ?? "Sistema",
                request.DatosAdicionales
            );

            if (!resultadoConfirmacion.Succeeded)
            {
                _logger.LogWarning("Error al confirmar reservación: {Error}", resultadoConfirmacion.ErrorMessage);
                return Result<ReservacionDto>.Failure(resultadoConfirmacion.ErrorMessage);
            }

            // 6. Guardar cambios
            await _reservacionRepository.ActualizarAsync(reservacion);

            // 7. Enviar notificaciones
            if (request.NotificarCliente)
            {
                await EnviarNotificacionConfirmacionAsync(reservacion);
            }

            _logger.LogInformation("Reservación confirmada exitosamente: {CodigoReservacion}", reservacion.CodigoReservacion);

            // 8. Mapear y retornar
            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
            return Result<ReservacionDto>.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al confirmar reservación: ID={ReservacionId}", request.ReservacionId);
            return Result<ReservacionDto>.Failure("Error interno del servidor al confirmar la reservación");
        }
    }

    private async Task EnviarNotificacionConfirmacionAsync(Reservacion reservacion)
    {
        try
        {
            await _notificacionService.EnviarNotificacionAsync(new NotificacionReservacionConfirmada
            {
                Email = reservacion.Email,
                Telefono = reservacion.Telefono,
                NombreCliente = reservacion.NombreCliente,
                CodigoReservacion = reservacion.CodigoReservacion,
                FechaHoraReservacion = reservacion.FechaHoraReservacion,
                NumeroPersonas = reservacion.NumeroPersonas,
                FechaConfirmacion = reservacion.FechaConfirmacion!.Value,
                MetodoConfirmacion = reservacion.MetodoConfirmacion!,
                NotasConfirmacion = reservacion.NotasConfirmacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificación de confirmación para reservación {CodigoReservacion}", 
                reservacion.CodigoReservacion);
        }
    }
} 