namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Handler para desactivar un cliente
/// Implementa eliminación lógica (soft delete) manteniendo integridad
/// </summary>
public class DesactivarClienteHandler : IRequestHandler<DesactivarClienteCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DesactivarClienteHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public DesactivarClienteHandler(
        IApplicationDbContext context,
        ILogger<DesactivarClienteHandler> logger,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(DesactivarClienteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando desactivación de cliente {ClienteId}", request.ClienteId);

            // 1. Obtener el cliente con sus datos relacionados
            var cliente = await _context.Clientes
                .Include(c => c.TarjetasFidelizacion)
                .Include(c => c.Reservaciones.Where(r => r.FechaHora > DateTime.UtcNow))
                .Include(c => c.Facturas.Where(f => f.Estado == EstadoFactura.Pendiente))
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure("El cliente especificado no existe.");
            }

            // 2. Verificar que el cliente esté activo
            if (!cliente.Activo)
            {
                _logger.LogWarning("Cliente {ClienteId} ya está desactivado", request.ClienteId);
                return Result.Failure("El cliente ya está desactivado.");
            }

            // 3. Validaciones de negocio adicionales
            var validacionResult = await ValidarDesactivacion(cliente);
            if (!validacionResult.IsSuccess)
            {
                return validacionResult;
            }

            // 4. Desactivar el cliente
            var estadoAnterior = cliente.Activo;
            cliente.Activo = false;
            cliente.FechaDesactivacion = DateTime.UtcNow;
            cliente.MotivoDesactivacion = request.MotivoDesactivacion;
            cliente.DesactivadoPor = request.DesactivadoPor;
            cliente.NotasDesactivacion = request.NotasAdicionales;

            // 5. Manejar reactivación automática si se especifica
            if (request.FechaReactivacion.HasValue)
            {
                cliente.FechaReactivacionProgramada = request.FechaReactivacion.Value;
                _logger.LogInformation("Cliente {ClienteId} programado para reactivación automática en {Fecha}", 
                    request.ClienteId, request.FechaReactivacion.Value);
            }

            // 6. Desactivar tarjetas de fidelización activas
            await DesactivarTarjetasFidelizacion(cliente);

            // 7. Cancelar reservaciones futuras si corresponde
            await CancelarReservacionesFuturas(cliente, request.MotivoDesactivacion);

            // 8. Guardar cambios
            await _context.SaveChangesAsync(cancellationToken);

            // 9. Notificar al cliente si se solicita
            if (request.NotificarCliente)
            {
                await NotificarDesactivacionCliente(cliente, request.MotivoDesactivacion);
            }

            // 10. Registrar auditoría
            await RegistrarAuditoriaDesactivacion(cliente, estadoAnterior, request);

            _logger.LogInformation("Cliente {ClienteId} desactivado exitosamente", request.ClienteId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar cliente {ClienteId}", request.ClienteId);
            return Result.Failure("Error interno al desactivar el cliente.");
        }
    }

    private async Task<Result> ValidarDesactivacion(Cliente cliente)
    {
        // Verificar reservaciones activas
        var reservacionesActivas = cliente.Reservaciones
            .Where(r => r.FechaHora > DateTime.UtcNow && 
                       (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente))
            .ToList();

        if (reservacionesActivas.Any())
        {
            return Result.Failure($"El cliente tiene {reservacionesActivas.Count} reservaciones activas que deben cancelarse primero.");
        }

        // Verificar facturas pendientes
        var facturasPendientes = cliente.Facturas
            .Where(f => f.Estado == EstadoFactura.Pendiente)
            .ToList();

        if (facturasPendientes.Any())
        {
            return Result.Failure($"El cliente tiene {facturasPendientes.Count} facturas pendientes de pago.");
        }

        return Result.Success();
    }

    private async Task DesactivarTarjetasFidelizacion(Cliente cliente)
    {
        var tarjetasActivas = cliente.TarjetasFidelizacion
            .Where(t => t.Activa)
            .ToList();

        foreach (var tarjeta in tarjetasActivas)
        {
            tarjeta.Activa = false;
            tarjeta.FechaDesactivacion = DateTime.UtcNow;
            tarjeta.MotivoDesactivacion = "Cliente desactivado";

            _logger.LogInformation("Tarjeta de fidelización {TarjetaId} desactivada automáticamente", tarjeta.Id);
        }

        if (tarjetasActivas.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task CancelarReservacionesFuturas(Cliente cliente, string motivo)
    {
        var reservacionesFuturas = cliente.Reservaciones
            .Where(r => r.FechaHora > DateTime.UtcNow && 
                       (r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente))
            .ToList();

        foreach (var reservacion in reservacionesFuturas)
        {
            reservacion.Estado = EstadoReservacion.Cancelada;
            reservacion.MotivoCancelacion = $"Cliente desactivado: {motivo}";
            reservacion.FechaCancelacion = DateTime.UtcNow;
            reservacion.CanceladoPor = "Sistema";

            _logger.LogInformation("Reservación {ReservacionId} cancelada automáticamente", reservacion.Id);
        }

        if (reservacionesFuturas.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private async Task NotificarDesactivacionCliente(Cliente cliente, string motivo)
    {
        try
        {
            // Email de notificación
            var emailContent = $@"
                <h2>Cuenta Desactivada</h2>
                <p>Estimado/a {cliente.Nombre},</p>
                <p>Su cuenta en RestaurantePro ha sido desactivada.</p>
                <p><strong>Motivo:</strong> {motivo}</p>
                <p>Si considera que esto es un error, por favor contacte con nuestro servicio al cliente.</p>
                <p>Atentamente,<br>Equipo RestaurantePro</p>";

            await _emailService.SendEmailAsync(
                cliente.Email,
                "Cuenta Desactivada - RestaurantePro",
                emailContent);

            // Notificación en sistema
            await _notificationService.CreateNotificationAsync(
                "Cuenta Desactivada",
                $"Su cuenta ha sido desactivada. Motivo: {motivo}",
                cliente.Id,
                NotificationType.CuentaCliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar desactivación de cliente {ClienteId}", cliente.Id);
        }
    }

    private async Task RegistrarAuditoriaDesactivacion(Cliente cliente, bool estadoAnterior, DesactivarClienteCommand request)
    {
        try
        {
            var auditoria = new AuditoriaCliente
            {
                ClienteId = cliente.Id,
                Accion = "Desactivación",
                EstadoAnterior = estadoAnterior ? "Activo" : "Inactivo",
                EstadoNuevo = "Inactivo",
                Detalles = $"Motivo: {request.MotivoDesactivacion}. Notas: {request.NotasAdicionales}",
                RealizadoPor = request.DesactivadoPor,
                FechaAccion = DateTime.UtcNow
            };

            _context.AuditoriasClientes.Add(auditoria);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoría de desactivación {ClienteId}", cliente.Id);
        }
    }
} 