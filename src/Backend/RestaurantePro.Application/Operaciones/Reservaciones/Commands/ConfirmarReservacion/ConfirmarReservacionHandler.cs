namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

/// <summary>
/// Handler para confirmar reservaciones con validaciones de negocio completas
/// Gestiona el proceso completo de confirmación con notificaciones automáticas
/// </summary>
public class ConfirmarReservacionHandler : IRequestHandler<ConfirmarReservacionCommand, Result<ConfirmarReservacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfirmarReservacionHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommunicationService _notificacionService;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmarReservacionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ConfirmarReservacionHandler> logger,
        ICurrentUserService currentUserService,
        ICommunicationService notificacionService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConfirmarReservacionDto>> Handle(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🎟️ Iniciando confirmación de reservación - ID: {ReservacionId}, Código: {CodigoReservacion}",
            request.ReservacionId, request.CodigoReservacion);

        try
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // 1. Obtener reservación
            var reservacionResult = await ObtenerReservacion(request, cancellationToken);
            if (!reservacionResult.Succeeded)
            {
                return Result.Failure<ConfirmarReservacionDto>(reservacionResult.Error!);
            }

            var reservacion = reservacionResult.Value;

            // 2. Validar reglas de negocio adicionales
            var validacionResult = await ValidarReglasNegocio(reservacion, cancellationToken);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure<ConfirmarReservacionDto>(validacionResult.Error!);
            }

            // 3. Confirmar la reservación
            await ConfirmarReservacion(reservacion, request, cancellationToken);

            // 4. Actualizar estado de la mesa
            await ActualizarEstadoMesa(reservacion, cancellationToken);

            // 5. Registrar auditoría
            await RegistrarAuditoria(reservacion, request, cancellationToken);

            // 6. Notificar cliente si es necesario
            if (request.NotificarCliente)
            {
                await NotificarConfirmacion(reservacion, request, cancellationToken);
            }

            // 7. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 8. Crear respuesta
            var response = CrearRespuesta(reservacion, request);

            _logger.LogInformation("✅ Reservación confirmada exitosamente: {ReservacionId}", reservacion.Id);
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al confirmar reservación - ID: {ReservacionId}: {ErrorMessage}", 
                request.ReservacionId, ex.Message);
            return Result.Failure<ConfirmarReservacionDto>($"Error interno al confirmar la reservación: {ex.Message}");
        }
    }

    #region Métodos privados

    private async Task<Result<Reservacion>> ObtenerReservacion(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        Reservacion? reservacion = null;

        if (request.ReservacionId != Guid.Empty)
        {
            reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.Id == request.ReservacionId, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.CodigoReservacion))
        {
            reservacion = await _context.Reservaciones
                .Include(r => r.Mesa)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.CodigoReservacion == request.CodigoReservacion, cancellationToken);
        }

            if (reservacion == null)
        {
            return Result.Failure<Reservacion>("La reservación especificada no existe.");
        }

        return Result.Success(reservacion);
    }

    private async Task<Result> ValidarReglasNegocio(Reservacion reservacion, CancellationToken cancellationToken)
    {
        // Validar que esté en estado pendiente
        if (reservacion.Estado != EstadoReservacionDomain.Pendiente)
        {
            return Result.Failure($"La reservación no puede ser confirmada. Estado actual: {reservacion.Estado}");
        }

        // Validar tiempo límite (2 horas antes de la reservación)
        var tiempoLimite = reservacion.FechaHora.AddHours(-2);
        if (DateTime.UtcNow > tiempoLimite)
        {
            return Result.Failure("Ha excedido el tiempo límite para confirmar la reservación.");
        }

        // Validar disponibilidad de mesa
        var conflictos = await _context.Reservaciones
            .Where(r => r.Id != reservacion.Id &&
                       r.MesaId == reservacion.MesaId &&
                       r.Estado == EstadoReservacionDomain.Confirmada &&
                       r.FechaHora.Date == reservacion.FechaHora.Date &&
                       // Verificar solapamiento de horarios (asumiendo 2 horas por reservación)
                       ((r.FechaHora <= reservacion.FechaHora && r.FechaHora.AddHours(2) > reservacion.FechaHora) ||
                        (reservacion.FechaHora <= r.FechaHora && reservacion.FechaHora.AddHours(2) > r.FechaHora)))
            .AnyAsync(cancellationToken);

        if (conflictos)
        {
            return Result.Failure("La mesa ya no está disponible para la fecha y hora de la reservación.");
        }

        return Result.Success();
    }

    private async Task ConfirmarReservacion(Reservacion reservacion, ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        // Actualizar estado y datos de confirmación
        reservacion.Estado = EstadoReservacionDomain.Confirmada;
        reservacion.FechaConfirmacion = DateTime.UtcNow;
        reservacion.MetodoConfirmacion = request.MetodoConfirmacion;
        reservacion.ConfirmadoPor = request.ConfirmadoPor;
        reservacion.NotasConfirmacion = request.NotasConfirmacion;
        reservacion.FechaUltimaActualizacion = DateTime.UtcNow;
        reservacion.ActualizadoPor = _currentUserService.UserId;

        // Agregar datos adicionales si están presentes
        if (request.DatosAdicionales?.Any() == true)
        {
            reservacion.DatosAdicionales = JsonSerializer.Serialize(request.DatosAdicionales);
        }

        _context.Reservaciones.Update(reservacion);
    }

    private async Task ActualizarEstadoMesa(Reservacion reservacion, CancellationToken cancellationToken)
    {
        if (reservacion.Mesa != null)
        {
            // Si la reservación es para hoy y pronto, marcar mesa como reservada
            var horasHastaReservacion = (reservacion.FechaHora - DateTime.UtcNow).TotalHours;
            
            if (horasHastaReservacion <= 24 && horasHastaReservacion > 0)
            {
                reservacion.Mesa.Estado = EstadoMesa.Reservada;
                reservacion.Mesa.FechaUltimaActualizacion = DateTime.UtcNow;
                _context.Mesas.Update(reservacion.Mesa);
            }
        }
    }

    private async Task RegistrarAuditoria(Reservacion reservacion, ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auditoria = new RegistroAuditoria
            {
                EntidadTipo = nameof(Reservacion),
                EntidadId = reservacion.Id.ToString(),
                Accion = "Confirmación Reservación",
                ValoresAnteriores = JsonSerializer.Serialize(new { Estado = "Pendiente" }),
                ValoresNuevos = JsonSerializer.Serialize(new { Estado = "Confirmada", MetodoConfirmacion = request.MetodoConfirmacion }),
                Motivo = $"Confirmación vía {request.MetodoConfirmacion}",
                UsuarioId = _currentUserService.UserId,
                Fecha = DateTime.UtcNow,
                DatosAdicionales = request.DatosAdicionales != null ? JsonSerializer.Serialize(request.DatosAdicionales) : null
            };

            _context.RegistrosAuditoria.Add(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría para confirmación de reservación {ReservacionId}", reservacion.Id);
        }
    }

    private async Task NotificarConfirmacion(Reservacion reservacion, ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mensaje = $"Su reservación #{reservacion.CodigoReservacion} ha sido confirmada para el {reservacion.FechaHora:dd/MM/yyyy 'a las' HH:mm} en la Mesa {reservacion.Mesa?.Numero}.";
            
            if (!string.IsNullOrEmpty(request.NotasConfirmacion))
            {
                mensaje += $" Notas: {request.NotasConfirmacion}";
            }

            // Notificar al cliente
            if (reservacion.Cliente != null)
            {
                await _notificacionService.EnviarNotificacionAsync(
                    destinatarios: new[] { reservacion.Cliente.Email },
                    titulo: "Reservación Confirmada",
                    mensaje: mensaje,
                    tipo: TipoComunicacion.ReservacionConfirmada,
                    cancellationToken: cancellationToken
                );
            }

            // Notificar internamente
            await _notificacionService.EnviarNotificacionAsync(
                destinatarios: new[] { "recepcion@restaurante.com" },
                titulo: "Reservación Confirmada",
                mensaje: $"Reservación {reservacion.CodigoReservacion} confirmada vía {request.MetodoConfirmacion}",
                tipo: TipoComunicacion.Informacion,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificación de confirmación para reservación {ReservacionId}", reservacion.Id);
        }
    }

    private ConfirmarReservacionDto CrearRespuesta(Reservacion reservacion, ConfirmarReservacionCommand request)
    {
        return new ConfirmarReservacionDto
        {
            ReservacionId = reservacion.Id,
            CodigoReservacion = reservacion.CodigoReservacion,
            Estado = reservacion.Estado,
            FechaConfirmacion = reservacion.FechaConfirmacion!.Value,
            MetodoConfirmacion = reservacion.MetodoConfirmacion!,
            ConfirmadoPor = reservacion.ConfirmadoPor,
            NotasConfirmacion = reservacion.NotasConfirmacion,
            ConfirmacionExitosa = true,
            MensajeConfirmacion = $"Reservación #{reservacion.CodigoReservacion} confirmada exitosamente para el {reservacion.FechaHora:dd/MM/yyyy 'a las' HH:mm}"
        };
    }

    #endregion
}

/// <summary>
/// DTO de respuesta para la confirmación de reservación
/// </summary>
public class ConfirmarReservacionDto
{
    public Guid ReservacionId { get; set; }
    public string CodigoReservacion { get; set; } = string.Empty;
    public EstadoReservacionDomain Estado { get; set; }
    public DateTime FechaConfirmacion { get; set; }
    public string MetodoConfirmacion { get; set; } = string.Empty;
    public string? ConfirmadoPor { get; set; }
    public string? NotasConfirmacion { get; set; }
    public bool ConfirmacionExitosa { get; set; }
    public string? MensajeConfirmacion { get; set; }
} 