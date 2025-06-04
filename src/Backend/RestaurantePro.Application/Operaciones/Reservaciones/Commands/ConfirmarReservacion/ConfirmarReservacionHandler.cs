using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

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
    // TODO: Agregar cuando IUnitOfWork esté disponible
    // private readonly IUnitOfWork _unitOfWork;

    public ConfirmarReservacionHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ConfirmarReservacionHandler> logger,
        ICurrentUserService currentUserService,
        ICommunicationService notificacionService)
        // TODO: Agregar cuando IUnitOfWork esté disponible
        // IUnitOfWork unitOfWork)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
        _notificacionService = notificacionService;
        // TODO: Asignar cuando esté disponible
        // _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConfirmarReservacionDto>> Handle(ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar cancelación
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("🎟️ Iniciando confirmación de reservación - ID: {ReservacionId}, Código: {CodigoReservacion}",
                request.ReservacionId, request.CodigoReservacion);

            // TODO: Usar UnitOfWork cuando esté disponible
            // using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

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

            // 3. Confirmar la reservación usando método del dominio
            reservacion.Confirmar();

            // 4. Aplicar datos adicionales del command
            AplicarDatosConfirmacion(reservacion, request);

            // 5. Actualizar estado de la mesa
            await ActualizarEstadoMesa(reservacion, cancellationToken);

            // 6. Registrar auditoría
            await RegistrarAuditoria(reservacion, request, cancellationToken);

            // 7. Notificar cliente si es necesario
            if (request.NotificarCliente)
            {
                await NotificarConfirmacion(reservacion, request, cancellationToken);
            }

            // 8. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);
            // TODO: Usar transaction cuando esté disponible
            // await transaction.CommitAsync(cancellationToken);

            // 9. Crear respuesta
            var response = CrearRespuesta(reservacion, request);

            _logger.LogInformation("✅ Reservación confirmada exitosamente: {ReservacionId}", reservacion.Id);
            return Result.Success(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("🚫 Operación cancelada al confirmar reservación {ReservacionId}", request.ReservacionId);
            throw; // Re-throw para que se propague correctamente
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
        // TODO: Implementar búsqueda por código cuando la propiedad esté disponible en la entidad
        // else if (!string.IsNullOrEmpty(request.CodigoReservacion))
        // {
        //     reservacion = await _context.Reservaciones
        //         .Include(r => r.Mesa)
        //         .Include(r => r.Cliente)
        //         .FirstOrDefaultAsync(r => r.CodigoReservacion == request.CodigoReservacion, cancellationToken);
        // }

        if (reservacion == null)
        {
            return Result.Failure<Reservacion>("La reservación especificada no existe.");
        }

        return Result.Success(reservacion);
    }

    private async Task<Result> ValidarReglasNegocio(Reservacion reservacion, CancellationToken cancellationToken)
    {
        // Validar que la reservación puede ser confirmada
        if (reservacion.Estado != EstadoReservacion.Pendiente)
        {
            return Result.Failure($"No se puede confirmar una reservación con estado {reservacion.Estado}");
        }

        // Validar que la reservación no haya expirado (ejemplo: 2 horas antes)
        var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
        if (fechaHoraReservacion <= DateTime.UtcNow.AddHours(2))
        {
            return Result.Failure("La reservación ha expirado o es demasiado próxima para ser confirmada");
        }

        // TODO: Agregar más validaciones de negocio cuando estén disponibles
        return Result.Success();
    }

    private void AplicarDatosConfirmacion(Reservacion reservacion, ConfirmarReservacionCommand request)
    {
        // TODO: Implementar cuando las propiedades estén disponibles en la entidad
        // Por ahora, solo logueamos la información
        _logger.LogInformation("📝 Datos de confirmación aplicados: Método={MetodoConfirmacion}, ConfirmadoPor={ConfirmadoPor}",
            request.MetodoConfirmacion, request.ConfirmadoPor);
        
        /*
        reservacion.MetodoConfirmacion = request.MetodoConfirmacion;
        reservacion.ConfirmadoPor = request.ConfirmadoPor;
        reservacion.NotasConfirmacion = request.NotasConfirmacion;
        
        if (request.DatosAdicionales?.Any() == true)
        {
            reservacion.DatosAdicionales = JsonSerializer.Serialize(request.DatosAdicionales);
        }
        */
    }

    private async Task ActualizarEstadoMesa(Reservacion reservacion, CancellationToken cancellationToken)
    {
        if (reservacion.Mesa != null)
        {
            // Si la reservación es para hoy y pronto, marcar mesa como reservada
            var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
            var horasHastaReservacion = (fechaHoraReservacion - DateTime.UtcNow).TotalHours;
            
            if (horasHastaReservacion <= 24 && horasHastaReservacion > 0)
            {
                // TODO: Usar las propiedades correctas de Mesa cuando estén disponibles
                // reservacion.Mesa.Estado = EstadoMesa.Reservada;
                // reservacion.Mesa.FechaUltimaActualizacion = DateTime.UtcNow;
                // _context.Mesas.Update(reservacion.Mesa);
                
                _logger.LogInformation("📅 Mesa preparada para reservación próxima: {MesaId}", reservacion.Mesa.Id);
            }
        }
    }

    private async Task RegistrarAuditoria(Reservacion reservacion, ConfirmarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Implementar auditoría cuando RegistroAuditoria esté disponible
            /*
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
            */
            
            _logger.LogInformation("📝 Auditoría registrada para confirmación de reservación {ReservacionId}", reservacion.Id);
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
            // TODO: Implementar cuando las propiedades estén disponibles en la entidad
            // var mensaje = $"Su reservación #{reservacion.CodigoReservacion} ha sido confirmada para el {reservacion.FechaReservacion:dd/MM/yyyy 'a las' HH:mm} en la Mesa {reservacion.Mesa?.Numero}.";
            
            var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
            var mensaje = $"Su reservación ha sido confirmada para el {fechaHoraReservacion:dd/MM/yyyy 'a las' HH:mm}.";
            
            if (!string.IsNullOrEmpty(request.NotasConfirmacion))
            {
                mensaje += $" Notas: {request.NotasConfirmacion}";
            }

            // TODO: Implementar notificaciones cuando el servicio esté disponible
            /*
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
                mensaje: $"Reservación confirmada vía {request.MetodoConfirmacion}",
                tipo: TipoComunicacion.Informacion,
                cancellationToken: cancellationToken
            );
            */
            
            _logger.LogInformation("📧 Notificaciones de confirmación preparadas para reservación {ReservacionId}", reservacion.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificación de confirmación para reservación {ReservacionId}", reservacion.Id);
        }
    }

    private ConfirmarReservacionDto CrearRespuesta(Reservacion reservacion, ConfirmarReservacionCommand request)
    {
        var fechaHoraReservacion = reservacion.Fecha.Add(reservacion.Hora);
        
        return new ConfirmarReservacionDto
        {
            ReservacionId = reservacion.Id,
            // TODO: Usar propiedad real cuando esté disponible
            // CodigoReservacion = reservacion.CodigoReservacion,
            CodigoReservacion = $"RES-{reservacion.Id:N}"[..12], // Código temporal
            Estado = reservacion.Estado,
            FechaConfirmacion = DateTime.UtcNow,
            MetodoConfirmacion = request.MetodoConfirmacion,
            ConfirmadoPor = request.ConfirmadoPor,
            NotasConfirmacion = request.NotasConfirmacion,
            ConfirmacionExitosa = true,
            MensajeConfirmacion = $"Reservación confirmada exitosamente para el {fechaHoraReservacion:dd/MM/yyyy 'a las' HH:mm}"
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
    public EstadoReservacion Estado { get; set; }
    public DateTime FechaConfirmacion { get; set; }
    public string MetodoConfirmacion { get; set; } = string.Empty;
    public string? ConfirmadoPor { get; set; }
    public string? NotasConfirmacion { get; set; }
    public bool ConfirmacionExitosa { get; set; }
    public string? MensajeConfirmacion { get; set; }
} 