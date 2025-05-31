namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;

public class CancelarReservacionHandler : IRequestHandler<CancelarReservacionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelarReservacionHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public CancelarReservacionHandler(
        IApplicationDbContext context,
        ILogger<CancelarReservacionHandler> logger,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(CancelarReservacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando cancelación de reservación {ReservacionId}", request.ReservacionId);

            // 1. Obtener la reservación
            var reservacion = await _context.Reservaciones
                // TODO: Descomentar cuando las relaciones estén implementadas
                // .Include(r => r.Cliente)
                // .Include(r => r.Mesa)
                .FirstOrDefaultAsync(r => r.Id == request.ReservacionId, cancellationToken);

            if (reservacion == null)
            {
                _logger.LogWarning("Reservación {ReservacionId} no encontrada", request.ReservacionId);
                return Result.Failure("La reservación especificada no existe.");
            }

            // 2. Validar estado actual
            if (!PuedeSerCancelada(reservacion))
            {
                _logger.LogWarning("Reservación {ReservacionId} no puede ser cancelada en estado {Estado}", 
                    request.ReservacionId, reservacion.Estado);
                return Result.Failure("La reservación no puede ser cancelada en su estado actual.");
            }

            // 3. Validar política de cancelación (2 horas mínimo)
            if (!CumplePoliticaCancelacion(reservacion))
            {
                _logger.LogWarning("Reservación {ReservacionId} no cumple política de cancelación", request.ReservacionId);
                return Result.Failure("La cancelación debe realizarse con al menos 2 horas de anticipación.");
            }

            // 4. Actualizar estado de la reservación
            var estadoAnterior = reservacion.Estado;
            // TODO: Implementar método para cambiar estado cuando esté disponible en el dominio
            // reservacion.Estado = Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada;
            // TODO: Implementar cuando las propiedades estén disponibles
            // reservacion.MotivoCancelacion = request.MotivoCancelacion;
            // reservacion.FechaCancelacion = DateTime.UtcNow;
            // reservacion.CanceladoPor = request.CanceladoPor;

            // 5. Liberar la mesa si estaba asignada
            // TODO: Implementar cuando Mesa esté disponible
            /*
            if (reservacion.Mesa != null)
            {
                reservacion.Mesa.Estado = EstadoMesa.Disponible;
                _logger.LogInformation("Mesa {MesaNumero} liberada automáticamente", reservacion.Mesa.Numero);
            }
            */

            // 6. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // 7. Notificar al cliente si se solicita
            // TODO: Implementar cuando Cliente esté disponible
            /*
            if (request.NotificarCliente && reservacion.Cliente != null)
            {
                await NotificarCancelacionCliente(reservacion);
            }
            */

            // 8. Registrar auditoría
            await RegistrarAuditoriaCancelacion(reservacion, estadoAnterior, "Sistema"); // request.CanceladoPor);

            _logger.LogInformation("Reservación {ReservacionId} cancelada exitosamente", request.ReservacionId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reservación {ReservacionId}", request.ReservacionId);
            return Result.Failure("Error interno al cancelar la reservación.");
        }
    }

    private static bool PuedeSerCancelada(Reservacion reservacion)
    {
        return reservacion.Estado == Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada ||
               reservacion.Estado == Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente;
    }

    private static bool CumplePoliticaCancelacion(Reservacion reservacion)
    {
        // TODO: Implementar cuando FechaHora esté disponible
        // var tiempoAnticipacion = reservacion.FechaHora - DateTime.UtcNow;
        // return tiempoAnticipacion.TotalHours >= 2;
        return true; // Temporal
    }

    private async Task NotificarCancelacionCliente(Reservacion reservacion)
    {
        try
        {
            // TODO: Implementar cuando Cliente esté disponible
            /*
            // Email de cancelación
            var emailContent = $@"
                <h2>Reservación Cancelada</h2>
                <p>Estimado/a {reservacion.Cliente.Nombre},</p>
                <p>Su reservación ha sido cancelada:</p>
                <ul>
                    <li><strong>Fecha:</strong> {reservacion.FechaHora:dd/MM/yyyy HH:mm}</li>
                    <li><strong>Mesa:</strong> {reservacion.Mesa?.Numero}</li>
                    <li><strong>Comensales:</strong> {reservacion.NumeroComensales}</li>
                    <li><strong>Motivo:</strong> {reservacion.MotivoCancelacion}</li>
                </ul>
                <p>Lamentamos los inconvenientes. Puede realizar una nueva reservación cuando guste.</p>
                <p>Atentamente,<br>Equipo RestaurantePro</p>";

            await _emailService.SendEmailAsync(
                reservacion.Cliente.Email,
                "Reservación Cancelada - RestaurantePro",
                emailContent);

            // Notificación en sistema
            await _notificationService.CreateNotificationAsync(
                "Reservación Cancelada",
                $"Su reservación para el {reservacion.FechaHora:dd/MM/yyyy HH:mm} ha sido cancelada.",
                reservacion.ClienteId,
                NotificationType.Reservacion);
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar cancelación de reservación {ReservacionId}", 
                reservacion.Id);
        }
    }

    private async Task RegistrarAuditoriaCancelacion(Reservacion reservacion, Domain.Operaciones.Reservaciones.Enums.EstadoReservacion estadoAnterior, string canceladoPor)
    {
        try
        {
            // TODO: Implementar cuando AuditoriaReservacion esté disponible en el dominio
            /*
            var auditoria = new AuditoriaReservacion
            {
                ReservacionId = reservacion.Id,
                Accion = "Cancelación",
                EstadoAnterior = estadoAnterior.ToString(),
                EstadoNuevo = Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada.ToString(),
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