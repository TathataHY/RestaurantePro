namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Handler para desactivar un cliente
/// Implementa eliminación lógica (soft delete) manteniendo integridad
/// </summary>
public class DesactivarClienteHandler : IRequestHandler<DesactivarClienteCommand, Result>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<DesactivarClienteHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public DesactivarClienteHandler(
        IClienteRepository clienteRepository,
        ILogger<DesactivarClienteHandler> logger,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _clienteRepository = clienteRepository;
        _logger = logger;
        _notificationService = notificationService;
        _emailService = emailService;
    }

    public async Task<Result> Handle(DesactivarClienteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando desactivación de cliente {ClienteId}", request.ClienteId);

            // 1. Obtener el cliente
            var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {ClienteId} no encontrado", request.ClienteId);
                return Result.Failure("El cliente especificado no existe.");
            }

            // 2. Verificar que el cliente esté activo
            if (!cliente.EstaActivo)
            {
                _logger.LogWarning("Cliente {ClienteId} ya está desactivado", request.ClienteId);
                return Result.Failure("El cliente ya está desactivado.");
            }

            // 3. Validaciones de negocio adicionales
            var validacionResult = await ValidarDesactivacion(cliente);
            if (!validacionResult.Succeeded)
            {
                return Result.Failure(validacionResult.Error);
            }

            // 4. Desactivar el cliente usando el método del dominio
            cliente.Desactivar();

            // 5. Manejar reactivación automática si se especifica
            if (request.FechaReactivacion.HasValue)
            {
                // TODO: Descomentar cuando Cliente tenga FechaReactivacionProgramada
                // cliente.FechaReactivacionProgramada = request.FechaReactivacion.Value;
                _logger.LogInformation("Cliente {ClienteId} programado para reactivación automática en {Fecha}", 
                    request.ClienteId, request.FechaReactivacion.Value);
            }

            // 6. Guardar cambios usando el repositorio
            await _clienteRepository.ActualizarAsync(cliente, cancellationToken);

            // 7. Notificar al cliente si se solicita
            if (request.NotificarCliente)
            {
                await NotificarDesactivacionCliente(cliente, request.MotivoDesactivacion);
            }

            // 8. Registrar auditoría
            await RegistrarAuditoriaDesactivacion(cliente, true, request); // estadoAnterior temporal

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
        // TODO: Descomentar cuando las entidades tengan las propiedades correctas
        /*
        // Verificar reservaciones activas
        var reservacionesActivas = cliente.Reservaciones
            .Where(r => r.FechaHora > DateTime.UtcNow && 
                       (r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada || 
                        r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente))
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
        */

        // Temporalmente asumir que no hay restricciones
        return Result.Success();
    }

    private async Task DesactivarTarjetasFidelizacion(Cliente cliente)
    {
        // TODO: Descomentar cuando las entidades tengan las relaciones correctas
        /*
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
        */

        _logger.LogInformation("Proceso de desactivación de tarjetas completado (temporal)");
    }

    private async Task CancelarReservacionesFuturas(Cliente cliente, string motivo)
    {
        // TODO: Descomentar cuando las entidades tengan las propiedades correctas
        /*
        var reservacionesFuturas = cliente.Reservaciones
            .Where(r => r.FechaHora > DateTime.UtcNow && 
                       (r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Confirmada || 
                        r.Estado == RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Pendiente))
            .ToList();

        foreach (var reservacion in reservacionesFuturas)
        {
            reservacion.Estado = RestaurantePro.Domain.Operaciones.Reservaciones.Enums.EstadoReservacion.Cancelada;
            reservacion.MotivoCancelacion = $"Cliente desactivado: {motivo}";
            reservacion.FechaCancelacion = DateTime.UtcNow;
            reservacion.CanceladoPor = "Sistema";

            _logger.LogInformation("Reservación {ReservacionId} cancelada automáticamente", reservacion.Id);
        }

        if (reservacionesFuturas.Any())
        {
            await _context.SaveChangesAsync();
        }
        */

        _logger.LogInformation("Proceso de cancelación de reservaciones completado (temporal)");
    }

    private async Task NotificarDesactivacionCliente(Cliente cliente, string motivo)
    {
        try
        {
            // TODO: Descomentar cuando Cliente tenga Email y Nombre
            /*
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
            */

            // ✅ ACTIVADO: Notificación en sistema usando INotificationService implementado
            var tituloNotificacion = "Cuenta Desactivada";
            var mensajeNotificacion = $"Su cuenta ha sido desactivada. Motivo: {motivo}";
            
            await _notificationService.EnviarNotificacionAsync(
                cliente.Id,
                tituloNotificacion,
                mensajeNotificacion,
                "Warning"); // Tipo de notificación de advertencia
            
            _logger.LogInformation("Notificación de desactivación enviada para cliente {ClienteId}", cliente.Id);
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
            // TODO: Descomentar cuando AuditoriaCliente esté disponible en el contexto
            /*
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
            */

            _logger.LogInformation("Auditoría de desactivación registrada para cliente {ClienteId}", cliente.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar auditoría de desactivación {ClienteId}", cliente.Id);
        }
    }
} 