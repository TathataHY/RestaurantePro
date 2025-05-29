namespace RestaurantePro.Domain.Operaciones.Reservaciones.Builders;

/// <summary>
/// Builder para construir instancias de Reservacion paso a paso con validaciones fluidas.
/// Permite crear reservaciones complejas de manera segura y legible, validando disponibilidad
/// de mesas, horarios y otros requerimientos del negocio.
/// </summary>
public class ReservacionBuilder
{
    private Guid? _mesaId;
    private Guid? _clienteId;
    private DateTime? _fecha;
    private TimeSpan? _hora;
    private TimeSpan? _duracionEstimada;
    private int? _cantidadPersonas;
    private string? _telefono;
    private string? _email;
    private string? _observaciones;
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<ReservacionBuilder> _logger;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public ReservacionBuilder(INotificationManager notificationManager, ILogger<ReservacionBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Establece la mesa a reservar
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ParaMesa(Guid mesaId)
    {
        Guard.AgainstEmpty(mesaId, nameof(mesaId));
        _mesaId = mesaId;
        _logger.LogDebug("Mesa {MesaId} asignada a la reservación", mesaId);
        return this;
    }

    /// <summary>
    /// Establece el cliente que realiza la reservación
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ParaCliente(Guid clienteId)
    {
        Guard.AgainstEmpty(clienteId, nameof(clienteId));
        _clienteId = clienteId;
        _logger.LogDebug("Cliente {ClienteId} asignado a la reservación", clienteId);
        return this;
    }

    /// <summary>
    /// Establece la fecha de la reservación
    /// </summary>
    /// <param name="fecha">Fecha de la reservación</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ParaFecha(DateTime fecha)
    {
        // Validar que la fecha sea futura
        if (fecha.Date < DateTime.Now.Date)
        {
            _notificationManager.AddError("La fecha de reservación debe ser futura", "FechaReservacion");
            _logger.LogWarning("Intento de asignar fecha pasada: {Fecha}", fecha);
            return this;
        }

        // Validar que no sea más de 3 meses en el futuro
        if (fecha.Date > DateTime.Now.Date.AddMonths(3))
        {
            _notificationManager.AddError("No se pueden hacer reservaciones con más de 3 meses de anticipación", "FechaReservacion");
            _logger.LogWarning("Intento de asignar fecha muy lejana: {Fecha}", fecha);
            return this;
        }

        _fecha = fecha.Date;
        _logger.LogDebug("Fecha {Fecha} asignada a la reservación", fecha.Date);
        return this;
    }

    /// <summary>
    /// Establece la hora de la reservación
    /// </summary>
    /// <param name="hora">Hora de la reservación</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder AHora(TimeSpan hora)
    {
        // Validar horarios de operación (11:00 AM - 11:00 PM)
        var horaApertura = new TimeSpan(11, 0, 0);
        var horaCierre = new TimeSpan(23, 0, 0);

        if (hora < horaApertura || hora > horaCierre)
        {
            _notificationManager.AddError("La hora debe estar entre 11:00 AM y 11:00 PM", "HoraReservacion");
            _logger.LogWarning("Intento de asignar hora fuera del horario de operación: {Hora}", hora);
            return this;
        }

        // Validar que sea en intervalos de 15 minutos
        if (hora.Minutes % 15 != 0)
        {
            _notificationManager.AddError("La hora debe ser en intervalos de 15 minutos", "HoraReservacion");
            _logger.LogWarning("Intento de asignar hora no múltiplo de 15 minutos: {Hora}", hora);
            return this;
        }

        _hora = hora;
        _logger.LogDebug("Hora {Hora} asignada a la reservación", hora);
        return this;
    }

    /// <summary>
    /// Establece la duración estimada de la reservación
    /// </summary>
    /// <param name="duracion">Duración estimada</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ConDuracion(TimeSpan duracion)
    {
        // Validar duración mínima
        if (duracion.TotalMinutes < 15)
        {
            _notificationManager.AddError("La duración estimada debe ser de al menos 15 minutos", "DuracionEstimada");
            _logger.LogWarning("Intento de asignar duración muy corta: {Duracion}", duracion);
            return this;
        }

        // Validar duración máxima
        if (duracion.TotalHours > 4)
        {
            _notificationManager.AddError("La duración estimada no puede exceder 4 horas", "DuracionEstimada");
            _logger.LogWarning("Intento de asignar duración muy larga: {Duracion}", duracion);
            return this;
        }

        _duracionEstimada = duracion;
        _logger.LogDebug("Duración {Duracion} asignada a la reservación", duracion);
        return this;
    }

    /// <summary>
    /// Establece la cantidad de personas para la reservación
    /// </summary>
    /// <param name="cantidadPersonas">Número de personas</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ParaPersonas(int cantidadPersonas)
    {
        if (cantidadPersonas <= 0)
        {
            _notificationManager.AddError("La cantidad de personas debe ser mayor que cero", "CantidadPersonas");
            _logger.LogWarning("Intento de asignar cantidad de personas inválida: {Cantidad}", cantidadPersonas);
            return this;
        }

        if (cantidadPersonas > 20)
        {
            _notificationManager.AddError("Para grupos de más de 20 personas, contacte directamente al restaurante", "CantidadPersonas");
            _logger.LogWarning("Intento de asignar cantidad de personas muy alta: {Cantidad}", cantidadPersonas);
            return this;
        }

        _cantidadPersonas = cantidadPersonas;
        _logger.LogDebug("Cantidad de personas {Cantidad} asignada a la reservación", cantidadPersonas);
        return this;
    }

    /// <summary>
    /// Establece el teléfono de contacto
    /// </summary>
    /// <param name="telefono">Número de teléfono</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ConTelefono(string telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
        {
            _notificationManager.AddError("El teléfono de contacto es obligatorio", "Telefono");
            _logger.LogWarning("Intento de asignar teléfono vacío");
            return this;
        }

        // Validar formato básico de teléfono chileno
        var telefonoLimpio = telefono.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (telefonoLimpio.Length < 8 || telefonoLimpio.Length > 12)
        {
            _notificationManager.AddError("El teléfono debe tener entre 8 y 12 dígitos", "Telefono");
            _logger.LogWarning("Intento de asignar teléfono con formato inválido: {Telefono}", telefono);
            return this;
        }

        if (!telefonoLimpio.All(char.IsDigit))
        {
            _notificationManager.AddError("El teléfono solo puede contener números", "Telefono");
            _logger.LogWarning("Intento de asignar teléfono con caracteres no numéricos: {Telefono}", telefono);
            return this;
        }

        _telefono = telefono;
        _logger.LogDebug("Teléfono asignado a la reservación");
        return this;
    }

    /// <summary>
    /// Establece el email de contacto
    /// </summary>
    /// <param name="email">Dirección de email</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ConEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _notificationManager.AddError("El email de contacto es obligatorio", "Email");
            _logger.LogWarning("Intento de asignar email vacío");
            return this;
        }

        // Validación básica de email
        if (!email.Contains("@") || !email.Contains(".") || email.Length < 5)
        {
            _notificationManager.AddError("El email debe tener un formato válido", "Email");
            _logger.LogWarning("Intento de asignar email con formato inválido: {Email}", email);
            return this;
        }

        _email = email;
        _logger.LogDebug("Email asignado a la reservación");
        return this;
    }

    /// <summary>
    /// Establece observaciones especiales para la reservación
    /// </summary>
    /// <param name="observaciones">Observaciones o requerimientos especiales</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public ReservacionBuilder ConObservaciones(string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            if (observaciones.Length > 500)
            {
                _notificationManager.AddError("Las observaciones no pueden exceder 500 caracteres", "Observaciones");
                _logger.LogWarning("Intento de asignar observaciones muy largas");
                return this;
            }

            _observaciones = observaciones;
            _logger.LogDebug("Observaciones agregadas: {Observaciones}", observaciones[..Math.Min(50, observaciones.Length)]);
        }
        return this;
    }

    /// <summary>
    /// Construye la reservación validando todos los datos
    /// </summary>
    /// <returns>Resultado con la reservación creada o errores de validación</returns>
    public Result<Reservacion> Construir()
    {
        // NO limpiar notificaciones si ya hay errores acumulados
        if (!_notificationManager.HasErrors)
        {
            _notificationManager.CreateNewNotification();
        }

        try
        {
            // Validaciones obligatorias
            ValidarDatosObligatorios();

            // Si hay errores, retornar resultado fallido
            if (_notificationManager.HasErrors)
            {
                _logger.LogWarning("No se pudo construir la reservación debido a errores de validación");
                return Result.Failure<Reservacion>("Errores de validación en la construcción de la reservación");
            }

            // Construir fecha y hora completa
            var fechaCompleta = _fecha!.Value.Add(_hora!.Value);

            // Crear la reservación usando el método de fábrica
            var reservacion = Reservacion.Crear(
                _mesaId!.Value,
                _clienteId!.Value,
                fechaCompleta,
                _duracionEstimada!.Value,
                _cantidadPersonas!.Value,
                _telefono!,
                _email!,
                _observaciones);

            _logger.LogInformation("Reservación construida exitosamente para fecha {Fecha} y hora {Hora}",
                _fecha.Value, _hora.Value);

            return Result.Success(reservacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al construir la reservación");
            _notificationManager.AddError($"Error interno: {ex.Message}", "ErrorInterno");
            return Result.Failure<Reservacion>("Error interno al construir la reservación");
        }
    }

    /// <summary>
    /// Valida que todos los datos obligatorios estén presentes
    /// </summary>
    private void ValidarDatosObligatorios()
    {
        if (!_mesaId.HasValue)
        {
            _notificationManager.AddError("La mesa es obligatoria", "Mesa");
        }

        if (!_clienteId.HasValue)
        {
            _notificationManager.AddError("El cliente es obligatorio", "Cliente");
        }

        if (!_fecha.HasValue)
        {
            _notificationManager.AddError("La fecha es obligatoria", "Fecha");
        }

        if (!_hora.HasValue)
        {
            _notificationManager.AddError("La hora es obligatoria", "Hora");
        }

        if (!_duracionEstimada.HasValue)
        {
            _notificationManager.AddError("La duración estimada es obligatoria", "DuracionEstimada");
        }

        if (!_cantidadPersonas.HasValue)
        {
            _notificationManager.AddError("La cantidad de personas es obligatoria", "CantidadPersonas");
        }

        if (string.IsNullOrWhiteSpace(_telefono))
        {
            _notificationManager.AddError("El teléfono es obligatorio", "Telefono");
        }

        if (string.IsNullOrWhiteSpace(_email))
        {
            _notificationManager.AddError("El email es obligatorio", "Email");
        }
    }

    /// <summary>
    /// Reinicia el builder para permitir reutilización
    /// </summary>
    /// <returns>Builder reiniciado</returns>
    public ReservacionBuilder Reset()
    {
        _mesaId = null;
        _clienteId = null;
        _fecha = null;
        _hora = null;
        _duracionEstimada = null;
        _cantidadPersonas = null;
        _telefono = null;
        _email = null;
        _observaciones = null;

        // Limpiar notificaciones acumuladas
        _notificationManager.CreateNewNotification();

        _logger.LogDebug("ReservacionBuilder reiniciado");
        return this;
    }

    /// <summary>
    /// Método estático para crear una nueva instancia del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger</param>
    /// <returns>Nueva instancia del builder</returns>
    public static ReservacionBuilder Nuevo(INotificationManager notificationManager, ILogger<ReservacionBuilder> logger)
    {
        return new ReservacionBuilder(notificationManager, logger);
    }
} 