namespace RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;

/// <summary>
/// Handler para crear reservaciones
/// Gestiona validación de disponibilidad, asignación de mesas y confirmaciones
/// </summary>
public class CrearReservacionHandler : IRequestHandler<CrearReservacionCommand, Result<ReservacionDto>>
{
    private readonly IReservacionRepository _reservacionRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IServicioDisponibilidad _servicioDisponibilidad;
    private readonly IAsignadorMesas _asignadorMesas;
    private readonly INotificacionService _notificacionService;
    private readonly IGeneradorCodigosReservacion _generadorCodigos;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearReservacionHandler> _logger;
    private readonly ICurrentUserService _currentUser;

    public CrearReservacionHandler(
        IReservacionRepository reservacionRepository,
        IMesaRepository mesaRepository,
        IClienteRepository clienteRepository,
        IServicioDisponibilidad servicioDisponibilidad,
        IAsignadorMesas asignadorMesas,
        INotificacionService notificacionService,
        IGeneradorCodigosReservacion generadorCodigos,
        IMapper mapper,
        ILogger<CrearReservacionHandler> logger,
        ICurrentUserService currentUser)
    {
        _reservacionRepository = reservacionRepository;
        _mesaRepository = mesaRepository;
        _clienteRepository = clienteRepository;
        _servicioDisponibilidad = servicioDisponibilidad;
        _asignadorMesas = asignadorMesas;
        _notificacionService = notificacionService;
        _generadorCodigos = generadorCodigos;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<Result<ReservacionDto>> Handle(CrearReservacionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando creación de reservación para {NombreCliente} el {FechaHora} para {NumeroPersonas} personas", 
            request.NombreCliente, request.FechaHoraReservacion, request.NumeroPersonas);

        try
        {
            // 1. Verificar disponibilidad general del restaurante
            var disponibilidadGeneral = await _servicioDisponibilidad.VerificarDisponibilidadAsync(
                request.FechaHoraReservacion, request.DuracionEstimadaMinutos);

            if (!disponibilidadGeneral.Disponible)
            {
                _logger.LogWarning("No hay disponibilidad general para {FechaHora}: {Razon}", 
                    request.FechaHoraReservacion, disponibilidadGeneral.RazonNoDisponibilidad);
                return Result<ReservacionDto>.Failure(disponibilidadGeneral.RazonNoDisponibilidad);
            }

            // 2. Validar o encontrar cliente
            Cliente? cliente = null;
            if (request.ClienteId.HasValue)
            {
                cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId.Value);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente especificado no encontrado: {ClienteId}", request.ClienteId);
                    return Result<ReservacionDto>.Failure("El cliente especificado no existe");
                }
            }
            else
            {
                // Buscar cliente por teléfono o email
                cliente = await BuscarClienteExistente(request.Telefono, request.Email);
            }

            // 3. Verificar conflictos de reservaciones para el cliente
            if (cliente != null)
            {
                var conflicto = await _reservacionRepository.TieneReservacionConflictoAsync(
                    cliente.Id, request.FechaHoraReservacion, request.DuracionEstimadaMinutos);

                if (conflicto)
                {
                    _logger.LogWarning("Cliente {ClienteId} ya tiene una reservación en conflicto para {FechaHora}", 
                        cliente.Id, request.FechaHoraReservacion);
                    return Result<ReservacionDto>.Failure("El cliente ya tiene una reservación activa en horario conflictivo");
                }
            }

            // 4. Asignar mesa (si no se especificó una)
            Mesa? mesaAsignada = null;
            if (request.MesaId.HasValue)
            {
                // Verificar que la mesa específica esté disponible
                mesaAsignada = await _mesaRepository.ObtenerPorIdAsync(request.MesaId.Value);
                if (mesaAsignada == null)
                {
                    return Result<ReservacionDto>.Failure("La mesa especificada no existe");
                }

                var mesaDisponible = await _servicioDisponibilidad.VerificarDisponibilidadMesaAsync(
                    request.MesaId.Value, request.FechaHoraReservacion, request.DuracionEstimadaMinutos);

                if (!mesaDisponible.Disponible)
                {
                    return Result<ReservacionDto>.Failure($"La mesa {mesaAsignada.Numero} no está disponible: {mesaDisponible.RazonNoDisponibilidad}");
                }
            }
            else
            {
                // Asignar mesa automáticamente
                var parametrosAsignacion = new ParametrosAsignacionMesa
                {
                    NumeroPersonas = request.NumeroPersonas,
                    FechaHoraReservacion = request.FechaHoraReservacion,
                    DuracionEstimada = request.DuracionEstimadaMinutos,
                    TipoMesaPreferida = request.TipoMesaPreferida,
                    ZonaPreferida = request.ZonaPreferida,
                    OcasionEspecial = request.OcasionEspecial
                };

                var resultadoAsignacion = await _asignadorMesas.AsignarMesaOptimalAsync(parametrosAsignacion);
                if (!resultadoAsignacion.Succeeded)
                {
                    _logger.LogWarning("No se pudo asignar mesa automáticamente: {Error}", resultadoAsignacion.ErrorMessage);
                    return Result<ReservacionDto>.Failure($"No hay mesas disponibles: {resultadoAsignacion.ErrorMessage}");
                }

                mesaAsignada = resultadoAsignacion.Value;
                _logger.LogInformation("Mesa asignada automáticamente: {NumeroMesa} para reservación", mesaAsignada.Numero);
            }

            // 5. Generar código de reservación único
            var codigoReservacion = await _generadorCodigos.GenerarCodigoAsync(new ParametrosCodigo
            {
                FechaReservacion = request.FechaHoraReservacion,
                NumeroPersonas = request.NumeroPersonas,
                Canal = request.Canal
            });

            if (!codigoReservacion.Succeeded)
            {
                return Result<ReservacionDto>.Failure($"Error al generar código de reservación: {codigoReservacion.ErrorMessage}");
            }

            // 6. Crear la reservación
            var reservacion = new Reservacion(
                cliente?.Id,
                request.NombreCliente,
                request.Telefono,
                request.Email,
                request.FechaHoraReservacion,
                request.NumeroPersonas,
                mesaAsignada.Id,
                codigoReservacion.Value,
                request.DuracionEstimadaMinutos,
                request.Canal,
                request.Comentarios,
                request.OcasionEspecial,
                request.TipoMesaPreferida,
                request.ZonaPreferida,
                request.PreferenciasAlimentarias,
                request.RequiereConfirmacion,
                request.MontoAnticipo,
                request.MetodoPagoAnticipo,
                request.EsRecurrente,
                request.PatronRecurrencia,
                request.NotasInternas,
                _currentUser.UserId ?? "Sistema",
                request.DatosAdicionales
            );

            // 7. Procesar anticipo si aplica
            if (request.MontoAnticipo.HasValue && request.MontoAnticipo > 0)
            {
                var resultadoAnticipo = reservacion.RegistrarAnticipo(
                    request.MontoAnticipo.Value,
                    request.MetodoPagoAnticipo!,
                    _currentUser.UserId ?? "Sistema"
                );

                if (!resultadoAnticipo.Succeeded)
                {
                    return Result<ReservacionDto>.Failure($"Error al registrar anticipo: {resultadoAnticipo.ErrorMessage}");
                }
            }

            // 8. Reservar la mesa temporalmente
            var resultadoReservaMesa = mesaAsignada.ReservarTemporalmente(
                request.FechaHoraReservacion,
                request.DuracionEstimadaMinutos,
                reservacion.Id,
                _currentUser.UserId ?? "Sistema"
            );

            if (!resultadoReservaMesa.Succeeded)
            {
                return Result<ReservacionDto>.Failure($"Error al reservar mesa: {resultadoReservaMesa.ErrorMessage}");
            }

            // 9. Guardar en repositorio
            await _reservacionRepository.AgregarAsync(reservacion);
            await _mesaRepository.ActualizarAsync(mesaAsignada);

            // 10. Crear reservaciones recurrentes si aplica
            if (request.EsRecurrente)
            {
                await CrearReservacionesRecurrentes(reservacion, request);
            }

            // 11. Enviar notificaciones
            if (request.NotificarCliente)
            {
                await EnviarNotificacionesAsync(reservacion, cliente, mesaAsignada);
            }

            _logger.LogInformation("Reservación creada exitosamente: {CodigoReservacion} para {NombreCliente}", 
                reservacion.CodigoReservacion, request.NombreCliente);

            // 12. Mapear y retornar
            var reservacionDto = _mapper.Map<ReservacionDto>(reservacion);
            return Result<ReservacionDto>.Success(reservacionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear reservación para {NombreCliente}", request.NombreCliente);
            return Result<ReservacionDto>.Failure("Error interno del servidor al crear la reservación");
        }
    }

    private async Task<Cliente?> BuscarClienteExistente(string telefono, string? email)
    {
        // Buscar primero por teléfono
        var clientePorTelefono = await _clienteRepository.BuscarPorTelefonoAsync(telefono);
        if (clientePorTelefono != null)
        {
            return clientePorTelefono;
        }

        // Buscar por email si está disponible
        if (!string.IsNullOrEmpty(email))
        {
            var clientePorEmail = await _clienteRepository.BuscarPorEmailAsync(email);
            if (clientePorEmail != null)
            {
                return clientePorEmail;
            }
        }

        return null;
    }

    private async Task CrearReservacionesRecurrentes(Reservacion reservacionBase, CrearReservacionCommand request)
    {
        try
        {
            var generadorRecurrencia = new GeneradorReservacionesRecurrentes();
            var fechasRecurrentes = generadorRecurrencia.GenerarFechas(
                request.FechaHoraReservacion,
                request.PatronRecurrencia!,
                cantidadMaxima: 12 // Máximo 12 reservaciones recurrentes
            );

            foreach (var fecha in fechasRecurrentes)
            {
                // Verificar disponibilidad para cada fecha
                var disponible = await _servicioDisponibilidad.VerificarDisponibilidadAsync(fecha, request.DuracionEstimadaMinutos);
                if (disponible.Disponible)
                {
                    var reservacionRecurrente = reservacionBase.CrearCopiaRecurrente(fecha, _currentUser.UserId ?? "Sistema");
                    await _reservacionRepository.AgregarAsync(reservacionRecurrente);
                }
            }

            _logger.LogInformation("Reservaciones recurrentes creadas para {CodigoReservacion}", reservacionBase.CodigoReservacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al crear reservaciones recurrentes para {CodigoReservacion}", reservacionBase.CodigoReservacion);
            // No fallar la operación principal por errores en recurrencia
        }
    }

    private async Task EnviarNotificacionesAsync(Reservacion reservacion, Cliente? cliente, Mesa mesa)
    {
        try
        {
            // Notificación al cliente
            await _notificacionService.EnviarNotificacionAsync(new NotificacionReservacion
            {
                ClienteId = cliente?.Id,
                Email = reservacion.Email,
                Telefono = reservacion.Telefono,
                NombreCliente = reservacion.NombreCliente,
                CodigoReservacion = reservacion.CodigoReservacion,
                FechaHoraReservacion = reservacion.FechaHoraReservacion,
                NumeroPersonas = reservacion.NumeroPersonas,
                NumeroMesa = mesa.Numero,
                RequiereConfirmacion = reservacion.RequiereConfirmacion,
                MontoAnticipo = reservacion.MontoAnticipo,
                Comentarios = reservacion.Comentarios
            });

            // Notificación interna al personal
            await _notificacionService.EnviarNotificacionInternaAsync(new NotificacionReservacionInterna
            {
                CodigoReservacion = reservacion.CodigoReservacion,
                FechaHoraReservacion = reservacion.FechaHoraReservacion,
                NumeroPersonas = reservacion.NumeroPersonas,
                Mesa = mesa.Numero,
                OcasionEspecial = reservacion.OcasionEspecial,
                PreferenciasAlimentarias = reservacion.PreferenciasAlimentarias,
                NotasInternas = reservacion.NotasInternas,
                RequiereConfirmacion = reservacion.RequiereConfirmacion
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para reservación {CodigoReservacion}", reservacion.CodigoReservacion);
        }
    }
} 