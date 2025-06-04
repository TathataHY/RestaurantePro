namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

public class CancelarReservacionHandler : IRequestHandler<CancelarReservacionCommand, Result<ReservacionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelarReservacionHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public CancelarReservacionHandler(
        IApplicationDbContext context,
        ILogger<CancelarReservacionHandler> logger,
        INotificationService notificationService,
        IEmailService emailService,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<Result<ReservacionDto>> Handle(CancelarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar si se solicitó cancelación
            cancellationToken.ThrowIfCancellationRequested();
            
            _logger.LogInformation("Iniciando cancelación de reservación {ReservacionId}", request.ReservacionId);

            // 1. Obtener la reservación
            var reservacion = await _context.Reservaciones
                // TODO: Descomentar cuando las relaciones estén implementadas
                // .Include(r => r.Cliente)
                // .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.ReservacionId, cancellationToken);

            // Verificar nuevamente si se solicitó cancelación después de la operación de repositorio
            cancellationToken.ThrowIfCancellationRequested();

            if (reservacion == null)
            {
                _logger.LogWarning("Reservación {ReservacionId} no encontrada", request.ReservacionId);
                return Result.Failure<ReservacionDto>("La reservación no fue encontrada");
            }

            // 2. Validar estado actual
            if (!PuedeSerCancelada(reservacion))
            {
                var mensaje = reservacion.Estado switch
                {
                    EstadoReservacion.Cancelada => "La reservación ya está cancelada",
                    EstadoReservacion.Completada => "La reservación completada no puede ser cancelada",
                    _ => "La reservación no puede ser cancelada en su estado actual"
                };
                
                _logger.LogWarning("Reservación {ReservacionId} no puede ser cancelada en estado {Estado}", 
                    request.ReservacionId, reservacion.Estado);
                return Result.Failure<ReservacionDto>(mensaje);
            }

            // 3. Validar política de cancelación (2 horas mínimo)
            if (!CumplePoliticaCancelacion(reservacion))
            {
                _logger.LogWarning("Reservación {ReservacionId} no cumple política de cancelación", request.ReservacionId);
                return Result.Failure<ReservacionDto>("La cancelación debe realizarse con al menos 2 horas de anticipación según la política de cancelación");
            }

            // 4. Cancelar la reservación usando el método de dominio
            var estadoAnterior = reservacion.Estado;
            var motivoCancelacion = !string.IsNullOrWhiteSpace(request.MotivoDetalle) 
                ? request.MotivoDetalle 
                : "Cancelación solicitada";

            try
            {
                reservacion.Cancelar(motivoCancelacion);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("No se puede cancelar la reservación {ReservacionId}: {Error}", 
                    request.ReservacionId, ex.Message);
                return Result.Failure<ReservacionDto>(ex.Message);
            }

            // 5. Liberar la mesa si estaba asignada
            // TODO: Implementar cuando Mesa esté disponible
            /*
            if (reservacion.Mesa != null)
            {
                reservacion.Mesa.Estado = EstadoMesa.Disponible;
                _logger.LogInformation("Mesa {MesaNumero} liberada automáticamente", reservacion.Mesa.Numero);
            }
            */

            // Verificar nuevamente si se solicitó cancelación antes de guardar cambios
            cancellationToken.ThrowIfCancellationRequested();
            
            // 6. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);
            
            // Verificar nuevamente si se solicitó cancelación después de guardar cambios
            cancellationToken.ThrowIfCancellationRequested();

            // 7. Notificar al cliente si se solicita
            if (request.NotificarCliente)
            {
                await NotificarCancelacionCliente(reservacion);
                cancellationToken.ThrowIfCancellationRequested();
            }

            // 8. Registrar auditoría
            await RegistrarAuditoriaCancelacion(reservacion, estadoAnterior, request.CanceladoPor ?? "Sistema");
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Reservación {ReservacionId} cancelada exitosamente", request.ReservacionId);

            return Result.Success(_mapper.Map<ReservacionDto>(reservacion));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operación cancelada al cancelar reservación {ReservacionId}", request.ReservacionId);
            throw; // Re-lanzar para que las pruebas de cancelación funcionen
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reservación {ReservacionId}", request.ReservacionId);
            return Result.Failure<ReservacionDto>("Error interno al cancelar la reservación.");
        }
    }

    private static bool PuedeSerCancelada(Reservacion reservacion)
    {
        return reservacion.Estado == EstadoReservacion.Confirmada ||
               reservacion.Estado == EstadoReservacion.Pendiente;
    }

    private static bool CumplePoliticaCancelacion(Reservacion reservacion)
    {
        // Usar la propiedad calculada FechaReservacion que combina Fecha + Hora
        var tiempoAnticipacion = reservacion.FechaReservacion - DateTime.Now;
        return tiempoAnticipacion.TotalHours >= 2;
    }

    private async Task NotificarCancelacionCliente(Reservacion reservacion)
    {
        try
        {
            // Usar directamente las propiedades disponibles en la reservación
            string clienteEmail = reservacion.Email;
            string clienteNombre = "Cliente"; // Por ahora usar valor por defecto
            
            // TODO: Obtener nombre del cliente cuando la navegación esté disponible
            /*
            if (reservacion.Cliente != null)
            {
                clienteNombre = reservacion.Cliente.Nombre;
            }
            */

            if (!string.IsNullOrWhiteSpace(clienteEmail))
            {
                var emailContent = $@"
                    <h2>Reservación Cancelada</h2>
                    <p>Estimado/a {clienteNombre},</p>
                    <p>Su reservación ha sido cancelada.</p>
                    <p>Lamentamos los inconvenientes. Puede realizar una nueva reservación cuando guste.</p>
                    <p>Atentamente,<br>Equipo RestaurantePro</p>";

                await _emailService.SendEmailAsync(
                    clienteEmail,
                    "Reservación Cancelada - RestaurantePro",
                    emailContent);
            }

            // Notificación en sistema usando el ClienteId de la reservación
            await _notificationService.EnviarNotificacionAsync(
                reservacion.ClienteId,
                "Reservación Cancelada",
                "Su reservación ha sido cancelada.",
                "Reservacion");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar cancelación de reservación {ReservacionId}", 
                reservacion.Id);
        }
    }

    private async Task RegistrarAuditoriaCancelacion(Reservacion reservacion, EstadoReservacion estadoAnterior, string canceladoPor)
    {
        try
        {
            _logger.LogInformation("Auditoría: Reservación {ReservacionId} cancelada. Estado anterior: {EstadoAnterior}, Cancelado por: {CanceladoPor}", 
                reservacion.Id, estadoAnterior, canceladoPor);
                
            // TODO: Implementar cuando AuditoriaReservacion esté disponible en el dominio
            /*
            var auditoria = new AuditoriaReservacion
            {
                ReservacionId = reservacion.Id,
                Accion = "Cancelación",
                EstadoAnterior = estadoAnterior.ToString(),
                EstadoNuevo = EstadoReservacion.Cancelada.ToString(),
                Detalles = $"Motivo: {reservacion.MotivoCancelacion}",
                RealizadoPor = canceladoPor,
                FechaAccion = DateTime.UtcNow
            };

            _context.AuditoriasReservaciones.Add(auditoria);
            await _context.SaveChangesAsync();
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoría de cancelación {ReservacionId}", 
                reservacion.Id);
        }
    }
} 