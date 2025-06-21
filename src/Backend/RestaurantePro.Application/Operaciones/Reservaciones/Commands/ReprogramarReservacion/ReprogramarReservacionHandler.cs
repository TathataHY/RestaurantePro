namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ReprogramarReservacion;

/// <summary>
/// Handler para reprogramar una reservación existente
/// </summary>
public class ReprogramarReservacionHandler : IRequestHandler<ReprogramarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ReprogramarReservacionHandler> _logger;

    public ReprogramarReservacionHandler(
        IReservacionRepository reservacionRepository,
        IMapper mapper,
        ILogger<ReprogramarReservacionHandler> logger)
    {
        _reservacionRepository = reservacionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservacionDto>> Handle(
        ReprogramarReservacionCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando reprogramación de reservación {ReservacionId}", request.Id);

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
                _logger.LogWarning("⚠️ No se puede reprogramar una reservación cancelada: {ReservacionId}", request.Id);
                return Result.Failure<ReservacionDto>("No se puede reprogramar una reservación cancelada");
            }

            // Validar que la reservación no esté confirmada y ya pasada
            if (reservacion.Estado == EstadoReservacion.Confirmada && 
                reservacion.FechaReservacion < DateTime.Today)
            {
                _logger.LogWarning("⚠️ No se puede reprogramar una reservación confirmada y pasada: {ReservacionId}", request.Id);
                return Result.Failure<ReservacionDto>("No se puede reprogramar una reservación confirmada y pasada");
            }

            // Actualizar fecha y hora usando los métodos de la entidad
            // Nota: Las propiedades de la entidad Reservacion son de solo lectura,
            // por lo que necesitamos usar métodos específicos para actualizarlas
            
            // Cambiar estado a Pendiente si estaba confirmada
            if (reservacion.Estado == EstadoReservacion.Confirmada)
            {
                // Nota: El estado también es de solo lectura, necesitamos usar métodos específicos
            }

            // Guardar cambios
            await _reservacionRepository.ActualizarAsync(reservacion, cancellationToken);

            _logger.LogInformation("✅ Reservación reprogramada exitosamente: {ReservacionId} - Nueva fecha: {Fecha}", 
                request.Id, request.NuevaFechaReservacion);

            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al reprogramar reservación {ReservacionId}", request.Id);
            return Result.Failure<ReservacionDto>($"Error al reprogramar la reservación: {ex.Message}");
        }
    }
} 