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

            // NOTA: Según el diseño del dominio, las propiedades de fecha y hora son INMUTABLES.
            // Para "reprogramar" una reservación, debemos:
            // 1. Cancelar la reservación existente
            // 2. Crear una nueva reservación con la nueva fecha/hora
            
            // Cancelar la reservación existente
            reservacion.Cancelar($"Reprogramada: {request.MotivoReprogramacion}");
            
            // Crear una nueva reservación con la nueva fecha/hora
            var nuevaFechaHora = request.NuevaFechaReservacion.Add(request.NuevaHoraReservacion);
            var nuevaReservacion = Reservacion.Crear(
                mesaId: reservacion.MesaId,
                clienteId: reservacion.ClienteId,
                fecha: nuevaFechaHora,
                duracionEstimada: reservacion.DuracionEstimada,
                cantidadPersonas: request.NuevoNumeroPersonas ?? reservacion.CantidadPersonas,
                telefono: reservacion.Telefono,
                email: reservacion.Email,
                observaciones: !string.IsNullOrWhiteSpace(request.MotivoReprogramacion) 
                    ? $"Reprogramada: {request.MotivoReprogramacion}" 
                    : reservacion.Observaciones
            );
            
            // Guardar la nueva reservación
            await _reservacionRepository.AgregarAsync(nuevaReservacion, cancellationToken);
            
            // Guardar cambios de la reservación cancelada
            await _reservacionRepository.ActualizarAsync(reservacion, cancellationToken);

            _logger.LogInformation("✅ Reservación reprogramada exitosamente: {ReservacionId} -> {NuevaReservacionId} - Nueva fecha: {Fecha}", 
                request.Id, nuevaReservacion.Id, nuevaFechaHora);

            var reservacionDto = _mapper.Map<ReservacionDto>(nuevaReservacion);
            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al reprogramar reservación {ReservacionId}", request.Id);
            return Result.Failure<ReservacionDto>($"Error al reprogramar la reservación: {ex.Message}");
        }
    }
} 