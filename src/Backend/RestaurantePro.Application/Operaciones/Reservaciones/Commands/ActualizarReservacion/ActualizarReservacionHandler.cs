namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ActualizarReservacion;

/// <summary>
/// Handler para actualizar una reservación existente
/// </summary>
public class ActualizarReservacionHandler : IRequestHandler<ActualizarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ActualizarReservacionHandler> _logger;

    public ActualizarReservacionHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ActualizarReservacionHandler> logger)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservacionDto>> Handle(
        ActualizarReservacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando actualización de reservación {ReservacionId}", request.Id);

        try
        {
            // Buscar la reservación existente
            var reservacion = await _reservacionRepository.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (reservacion == null)
            {
                _logger.LogWarning("⚠️ Reservación no encontrada: {ReservacionId}", request.Id);
                return Result.Failure<ReservacionDto>("Reservación no encontrada");
            }

            // Validar que la reservación no esté cancelada
            if (reservacion.Estado == EstadoReservacion.Cancelada)
            {
                _logger.LogWarning("⚠️ No se puede actualizar una reservación cancelada: {ReservacionId}", request.Id);
                return Result.Failure<ReservacionDto>("No se puede actualizar una reservación cancelada");
            }

            // NOTA: Según el diseño del dominio, las propiedades de fecha, hora, 
            // cantidad de personas y observaciones son INMUTABLES una vez creada la reservación.
            // Solo se pueden actualizar la mesa y el estado.
            
            // Actualizar mesa si se proporcionó
            if (request.MesaId.HasValue && request.MesaId.Value != Guid.Empty && request.MesaId.Value != reservacion.MesaId)
            {
                try
                {
                    reservacion.CambiarMesa(request.MesaId.Value);
                    _logger.LogInformation("Mesa actualizada de {MesaAnterior} a {MesaNueva}", reservacion.MesaId, request.MesaId.Value);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("No se pudo cambiar la mesa: {Error}", ex.Message);
                    return Result.Failure<ReservacionDto>($"No se pudo cambiar la mesa: {ex.Message}");
                }
            }

            // Actualizar observaciones si se proporcionaron
            if (!string.IsNullOrWhiteSpace(request.Observaciones))
            {
                try
                {
                    reservacion.ActualizarObservaciones(request.Observaciones);
                    _logger.LogInformation("Observaciones actualizadas para reservación {ReservacionId}", request.Id);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning("No se pudieron actualizar las observaciones: {Error}", ex.Message);
                    return Result.Failure<ReservacionDto>($"No se pudieron actualizar las observaciones: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Error en las observaciones proporcionadas: {Error}", ex.Message);
                    return Result.Failure<ReservacionDto>($"Error en las observaciones: {ex.Message}");
                }
            }
            
            // Log de propiedades que no se pueden actualizar (para información del desarrollador)
            if (request.FechaReservacion != default || request.HoraReservacion != default || request.NumeroPersonas > 0)
            {
                _logger.LogInformation("⚠️ Propiedades solicitadas para actualización pero no implementadas en el dominio: Fecha={Fecha}, Hora={Hora}, Personas={Personas}", 
                    request.FechaReservacion, request.HoraReservacion, request.NumeroPersonas);
            }
            
            // Guardar cambios
            await _reservacionRepository.ActualizarAsync(reservacion, cancellationToken);

            _logger.LogInformation("✅ Reservación actualizada exitosamente: {ReservacionId}", request.Id);

            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al actualizar reservación {ReservacionId}", request.Id);
            return Result.Failure<ReservacionDto>($"Error al actualizar la reservación: {ex.Message}");
        }
    }
} 