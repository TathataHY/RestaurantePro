namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;

/// <summary>
/// Handler para modificar reservaciones existentes
/// Gestiona el proceso completo de modificación con validaciones de negocio
/// </summary>
public class ModificarReservacionHandler : IRequestHandler<ModificarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ModificarReservacionHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificacionService _notificacionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public ModificarReservacionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ModificarReservacionHandler> logger,
        ICurrentUserService currentUserService,
        INotificacionService notificacionService,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<ReservacionDto>> Handle(ModificarReservacionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando modificación de reservación - ID: {ReservacionId}, Motivo: {Motivo}",
            request.ReservacionId, request.MotivoModificacion);

        try
        {
            // 1. Obtener la reservación existente
            var reservacionResult = await ObtenerReservacionCompleta(request.ReservacionId, cancellationToken);
            if (!reservacionResult.Succeeded)
            {
                return Result.Failure<ReservacionDto>(reservacionResult.Error);
            }

            var reservacion = reservacionResult.Value;

            // 2. Verificar que se puede modificar
            var verificacionResult = VerificarPuedeModificar(reservacion);
            if (!verificacionResult.Succeeded)
            {
                return Result.Failure<ReservacionDto>(verificacionResult.Error);
            }

            // 3. Verificar disponibilidad de nueva mesa si cambió
            if (request.NuevaMesaId.HasValue && request.NuevaMesaId != reservacion.MesaId)
            {
                var disponibilidadResult = await VerificarDisponibilidadMesa(
                    request.NuevaMesaId.Value, 
                    request.NuevaFechaReservacion, 
                    request.NuevaHoraReservacion, 
                    request.ReservacionId,
                    cancellationToken);

                if (!disponibilidadResult.Succeeded)
                {
                    return Result.Failure<ReservacionDto>(disponibilidadResult.Error);
                }
            }

            // 4. Crear historial de cambios
            var historialCambios = CrearHistorialCambios(reservacion, request);

            // 5. Aplicar modificaciones
            AplicarModificaciones(reservacion, request);

            // 6. Registrar auditoría
            RegistrarAuditoria(reservacion, request, historialCambios);

            // 7. Guardar cambios
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 8. Enviar notificaciones
            await EnviarNotificacionesModificacion(reservacion, request, cancellationToken);

            // 9. Mapear y retornar
            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);

            _logger.LogInformation("✅ Reservación modificada exitosamente - ID: {ReservacionId}", 
                request.ReservacionId);

            return Result.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error modificando reservación {ReservacionId}: {Error}", 
                request.ReservacionId, ex.Message);
            return Result.Failure<ReservacionDto>("Error interno al modificar la reservación");
        }
    }

    private async Task<Result<Reservacion>> ObtenerReservacionCompleta(Guid reservacionId, CancellationToken cancellationToken)
    {
        var reservacion = await _context.Reservaciones
            .Include(r => r.Cliente)
            .Include(r => r.Mesa)
            .FirstOrDefaultAsync(r => r.Id == reservacionId, cancellationToken);

        if (reservacion == null)
        {
            _logger.LogWarning("⚠️ Reservación no encontrada: {ReservacionId}", reservacionId);
            return Result.Failure<Reservacion>("La reservación especificada no existe");
        }

        return Result.Success(reservacion);
    }

    private Result VerificarPuedeModificar(Reservacion reservacion)
    {
        // Verificar estado de la reservación
        if (reservacion.Estado == EstadoReservacion.Cancelada)
        {
            _logger.LogWarning("⚠️ Intento de modificar reservación cancelada: {ReservacionId}", reservacion.Id);
            return Result.Failure("No se puede modificar una reservación cancelada");
        }

        if (reservacion.Estado == EstadoReservacion.Completada)
        {
            _logger.LogWarning("⚠️ Intento de modificar reservación completada: {ReservacionId}", reservacion.Id);
            return Result.Failure("No se puede modificar una reservación completada");
        }

        // Verificar tiempo límite para modificaciones (ej: no menos de 2 horas antes)
        var fechaHoraReservacion = reservacion.FechaReservacion.Add(reservacion.HoraReservacion);
        var tiempoLimite = _dateTimeService.Now.AddHours(2);

        if (fechaHoraReservacion <= tiempoLimite)
        {
            _logger.LogWarning("⚠️ Intento de modificar reservación con muy poco tiempo de anticipación: {ReservacionId}", 
                reservacion.Id);
            return Result.Failure("No se puede modificar la reservación con menos de 2 horas de anticipación");
        }

        return Result.Success();
    }

    private async Task<Result> VerificarDisponibilidadMesa(
        Guid mesaId, 
        DateTime nuevaFecha, 
        TimeSpan nuevaHora,
        Guid reservacionIdExcluir,
        CancellationToken cancellationToken)
    {
        // Verificar que la mesa existe
        var mesa = await _context.Mesas.FindAsync(new object[] { mesaId }, cancellationToken);
        if (mesa == null)
        {
            return Result.Failure("La mesa especificada no existe");
        }

        // Verificar disponibilidad (excluyendo la reservación actual)
        var conflictos = await _context.Reservaciones
            .Where(r => r.MesaId == mesaId && 
                       r.Id != reservacionIdExcluir &&
                       r.FechaReservacion.Date == nuevaFecha.Date &&
                       r.Estado != EstadoReservacion.Cancelada)
            .ToListAsync(cancellationToken);

        foreach (var conflicto in conflictos)
        {
            var inicioConflicto = conflicto.HoraReservacion;
            var finConflicto = conflicto.HoraReservacion.Add(TimeSpan.FromHours(2)); // Duración estimada 2 horas

            var inicioNueva = nuevaHora;
            var finNueva = nuevaHora.Add(TimeSpan.FromHours(2));

            // Verificar solapamiento
            if (inicioNueva < finConflicto && finNueva > inicioConflicto)
            {
                _logger.LogWarning("⚠️ Conflicto de horario en mesa {MesaId} para {Fecha} {Hora}", 
                    mesaId, nuevaFecha.Date, nuevaHora);
                return Result.Failure("La mesa no está disponible en el horario solicitado");
            }
        }

        return Result.Success();
    }

    private string CrearHistorialCambios(Reservacion reservacionOriginal, ModificarReservacionCommand request)
    {
        var cambios = new List<string>();

        if (request.NuevaFechaReservacion != reservacionOriginal.FechaReservacion)
        {
            cambios.Add($"Fecha: {reservacionOriginal.FechaReservacion:dd/MM/yyyy} → {request.NuevaFechaReservacion:dd/MM/yyyy}");
        }

        if (request.NuevaHoraReservacion != reservacionOriginal.HoraReservacion)
        {
            cambios.Add($"Hora: {reservacionOriginal.HoraReservacion:hh\\:mm} → {request.NuevaHoraReservacion:hh\\:mm}");
        }

        if (request.NuevoNumeroPersonas != reservacionOriginal.NumeroPersonas)
        {
            cambios.Add($"Personas: {reservacionOriginal.NumeroPersonas} → {request.NuevoNumeroPersonas}");
        }

        if (request.NuevaMesaId.HasValue && request.NuevaMesaId != reservacionOriginal.MesaId)
        {
            cambios.Add($"Mesa: {reservacionOriginal.MesaId} → {request.NuevaMesaId}");
        }

        if (request.NuevoClienteId.HasValue && request.NuevoClienteId != reservacionOriginal.ClienteId)
        {
            cambios.Add($"Cliente: {reservacionOriginal.ClienteId} → {request.NuevoClienteId}");
        }

        return string.Join("; ", cambios);
    }

    private void AplicarModificaciones(Reservacion reservacion, ModificarReservacionCommand request)
    {
        reservacion.FechaReservacion = request.NuevaFechaReservacion;
        reservacion.HoraReservacion = request.NuevaHoraReservacion;
        reservacion.NumeroPersonas = request.NuevoNumeroPersonas;

        if (request.NuevaMesaId.HasValue)
        {
            reservacion.MesaId = request.NuevaMesaId.Value;
        }

        if (request.NuevoClienteId.HasValue)
        {
            reservacion.ClienteId = request.NuevoClienteId.Value;
        }

        // Actualizar metadatos
        reservacion.FechaModificacion = _dateTimeService.Now;
        reservacion.UsuarioModificacionId = _currentUserService.UserId;

        _logger.LogInformation("🔄 Modificaciones aplicadas a reservación {ReservacionId}", reservacion.Id);
    }

    private void RegistrarAuditoria(Reservacion reservacion, ModificarReservacionCommand request, string historialCambios)
    {
        // TODO: Implementar sistema de auditoría completo cuando esté disponible
        _logger.LogInformation("📝 Auditoría - Reservación {ReservacionId} modificada por usuario {UsuarioId}. " +
            "Motivo: {Motivo}. Cambios: {Cambios}",
            reservacion.Id, 
            request.UsuarioId,
            request.MotivoModificacion,
            historialCambios);
    }

    private async Task EnviarNotificacionesModificacion(
        Reservacion reservacion, 
        ModificarReservacionCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Notificar al cliente
            await _notificacionService.EnviarNotificacionModificacionReservacionAsync(
                reservacion.ClienteId,
                reservacion.Id,
                request.MotivoModificacion,
                cancellationToken);

            // Notificar al personal del restaurante
            await _notificacionService.EnviarNotificacionInternaModificacionAsync(
                reservacion.Id,
                request.MotivoModificacion,
                _currentUserService.UserId,
                cancellationToken);

            _logger.LogInformation("📧 Notificaciones de modificación enviadas para reservación {ReservacionId}", 
                reservacion.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error enviando notificaciones para reservación {ReservacionId}: {Error}", 
                reservacion.Id, ex.Message);
            // No fallar el proceso por errores de notificación
        }
    }
} 