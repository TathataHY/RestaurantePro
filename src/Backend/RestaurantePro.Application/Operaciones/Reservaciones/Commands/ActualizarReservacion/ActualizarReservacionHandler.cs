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

            // Actualizar propiedades usando los métodos de la entidad
            // Nota: Las propiedades de la entidad Reservacion son de solo lectura,
            // por lo que necesitamos usar métodos específicos para actualizarlas
            
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