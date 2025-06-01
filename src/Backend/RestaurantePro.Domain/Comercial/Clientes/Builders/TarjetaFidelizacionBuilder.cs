namespace RestaurantePro.Domain.Comercial.Clientes.Builders;

/// <summary>
/// Builder para construir instancias de TarjetaFidelizacion paso a paso con validaciones fluidas.
/// Permite crear tarjetas de fidelización de manera segura y legible.
/// </summary>
public class TarjetaFidelizacionBuilder
{
    private Guid? _clienteId;
    private string? _tipoTarjeta;
    private string? _numeroTarjeta;
    private int? _puntosIniciales;
    private NivelFidelizacion? _nivel;
    private DateTime? _fechaVencimiento;
    private bool _activa = true;
    private readonly List<Guid> _beneficios = new();
    private decimal _multiplicador = 1.0m;
    private string? _observaciones;
    private string? _datosAdicionales;
    private bool _incluirCodigoQR = false;
    
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<TarjetaFidelizacionBuilder> _logger;

    private Guid? _id;
    private bool? _estadoActivo;
    private int? _puntosDisponibles;

    /// <summary>
    /// Constructor del builder
    /// </summary>
    /// <param name="notificationManager">Manager de notificaciones</param>
    /// <param name="logger">Logger para eventos del builder</param>
    public TarjetaFidelizacionBuilder(INotificationManager notificationManager, ILogger<TarjetaFidelizacionBuilder> logger)
    {
        _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Factory method estático para crear una nueva instancia del builder
    /// </summary>
    public static TarjetaFidelizacionBuilder Nuevo(INotificationManager notificationManager, ILogger<TarjetaFidelizacionBuilder> logger)
    {
        return new TarjetaFidelizacionBuilder(notificationManager, logger);
    }

    /// <summary>
    /// Factory method estático para tests - crear builder sin dependencias
    /// </summary>
    public static TarjetaFidelizacionBuilder Crear()
    {
        var notificationManager = new SimpleNotificationManager();
        var logger = new SimpleLogger();
        return new TarjetaFidelizacionBuilder(notificationManager, logger);
    }

    /// <summary>
    /// Establece el cliente propietario de la tarjeta
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ParaCliente(Guid clienteId)
    {
        if (clienteId == Guid.Empty)
        {
            _notificationManager.AddError("El ID del cliente no puede estar vacío", "ClienteId", nameof(clienteId));
            return this;
        }

        _clienteId = clienteId;
        _logger.LogDebug("Cliente asignado a tarjeta: {ClienteId}", clienteId);
        return this;
    }

    /// <summary>
    /// Establece el tipo de tarjeta
    /// </summary>
    /// <param name="tipoTarjeta">Tipo de tarjeta</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConTipo(string tipoTarjeta)
    {
        if (string.IsNullOrWhiteSpace(tipoTarjeta))
        {
            _notificationManager.AddError("El tipo de tarjeta no puede estar vacío", "TipoTarjeta", nameof(tipoTarjeta));
            return this;
        }

        if (tipoTarjeta.Length > 50)
        {
            _notificationManager.AddError("El tipo de tarjeta no puede exceder 50 caracteres", "TipoTarjeta", nameof(tipoTarjeta));
            return this;
        }

        _tipoTarjeta = tipoTarjeta.Trim();
        _logger.LogDebug("Tipo de tarjeta establecido: {TipoTarjeta}", _tipoTarjeta);
        return this;
    }

    /// <summary>
    /// Establece el número de la tarjeta
    /// </summary>
    /// <param name="numero">Número de la tarjeta</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConNumero(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
        {
            _notificationManager.AddError("El número de tarjeta no puede estar vacío", "NumeroTarjeta", nameof(numero));
            return this;
        }

        if (numero.Length != 16)
        {
            _notificationManager.AddError("El número de tarjeta debe tener exactamente 16 caracteres", "NumeroTarjeta", nameof(numero));
            return this;
        }

        _numeroTarjeta = numero.Trim();
        _logger.LogDebug("Número de tarjeta establecido: {NumeroTarjeta}", _numeroTarjeta);
        return this;
    }

    /// <summary>
    /// Establece los puntos iniciales
    /// </summary>
    /// <param name="puntosIniciales">Puntos iniciales</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConPuntosIniciales(int puntosIniciales)
    {
        if (puntosIniciales < 0)
        {
            _notificationManager.AddError("Los puntos iniciales no pueden ser negativos", "PuntosIniciales", nameof(puntosIniciales));
            return this;
        }

        if (puntosIniciales > 10000)
        {
            _notificationManager.AddError("Los puntos iniciales no pueden ser mayores a 10,000", "PuntosIniciales", nameof(puntosIniciales));
            return this;
        }

        _puntosIniciales = puntosIniciales;
        _logger.LogDebug("Puntos iniciales establecidos: {PuntosIniciales}", puntosIniciales);
        return this;
    }

    /// <summary>
    /// Establece el nivel de fidelización
    /// </summary>
    /// <param name="nivel">Nivel de fidelización</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConNivel(NivelFidelizacion nivel)
    {
        _nivel = nivel;
        _logger.LogDebug("Nivel de fidelización establecido: {Nivel}", nivel);
        return this;
    }

    /// <summary>
    /// Establece la fecha de vencimiento
    /// </summary>
    /// <param name="fechaVencimiento">Fecha de vencimiento</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConVencimiento(DateTime fechaVencimiento)
    {
        if (fechaVencimiento <= DateTime.Now)
        {
            _notificationManager.AddError("La fecha de vencimiento debe ser futura", "FechaVencimiento", nameof(fechaVencimiento));
            return this;
        }

        if (fechaVencimiento > DateTime.Now.AddYears(10))
        {
            _notificationManager.AddError("La fecha de vencimiento no puede ser mayor a 10 años", "FechaVencimiento", nameof(fechaVencimiento));
            return this;
        }

        _fechaVencimiento = fechaVencimiento;
        _logger.LogDebug("Fecha de vencimiento establecida: {FechaVencimiento}", fechaVencimiento);
        return this;
    }

    /// <summary>
    /// Establece si la tarjeta está activa
    /// </summary>
    /// <param name="activa">Estado activo</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConEstado(bool activa)
    {
        _activa = activa;
        _logger.LogDebug("Estado de tarjeta establecido: {Activa}", activa);
        return this;
    }

    /// <summary>
    /// Añade beneficios a la tarjeta
    /// </summary>
    /// <param name="beneficios">Lista de IDs de beneficios</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConBeneficios(List<Guid> beneficios)
    {
        if (beneficios?.Any() == true)
        {
            foreach (var beneficioId in beneficios.Where(b => b != Guid.Empty))
            {
                if (!_beneficios.Contains(beneficioId))
                {
                    _beneficios.Add(beneficioId);
                }
            }
            _logger.LogDebug("Beneficios agregados: {CantidadBeneficios}", _beneficios.Count);
        }
        return this;
    }

    /// <summary>
    /// Establece el multiplicador de puntos
    /// </summary>
    /// <param name="multiplicador">Multiplicador</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConMultiplicador(decimal multiplicador)
    {
        if (multiplicador < 0.1m)
        {
            _notificationManager.AddError("El multiplicador no puede ser menor a 0.1", "Multiplicador", nameof(multiplicador));
            return this;
        }

        if (multiplicador > 10.0m)
        {
            _notificationManager.AddError("El multiplicador no puede ser mayor a 10.0", "Multiplicador", nameof(multiplicador));
            return this;
        }

        _multiplicador = multiplicador;
        _logger.LogDebug("Multiplicador establecido: {Multiplicador}", multiplicador);
        return this;
    }

    /// <summary>
    /// Establece observaciones
    /// </summary>
    /// <param name="observaciones">Observaciones</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConObservaciones(string observaciones)
    {
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            if (observaciones.Length > 500)
            {
                _notificationManager.AddError("Las observaciones no pueden exceder 500 caracteres", "Observaciones", nameof(observaciones));
                return this;
            }

            _observaciones = observaciones.Trim();
            _logger.LogDebug("Observaciones establecidas: {Longitud} caracteres", _observaciones.Length);
        }
        return this;
    }

    /// <summary>
    /// Establece datos adicionales
    /// </summary>
    /// <param name="datosAdicionales">Datos adicionales en formato JSON</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConDatosAdicionales(string datosAdicionales)
    {
        if (!string.IsNullOrWhiteSpace(datosAdicionales))
        {
            try
            {
                // Validar que sea JSON válido
                System.Text.Json.JsonDocument.Parse(datosAdicionales);
                _datosAdicionales = datosAdicionales.Trim();
                _logger.LogDebug("Datos adicionales establecidos como JSON válido");
            }
            catch (System.Text.Json.JsonException)
            {
                _notificationManager.AddError("Los datos adicionales deben ser JSON válido", "DatosAdicionales", nameof(datosAdicionales));
            }
        }
        return this;
    }

    /// <summary>
    /// Habilita la generación del código QR para la tarjeta
    /// </summary>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConCodigoQR()
    {
        _incluirCodigoQR = true;
        _logger.LogDebug("Código QR habilitado para la tarjeta");
        return this;
    }

    /// <summary>
    /// Establece el ID de la tarjeta (para tests)
    /// </summary>
    /// <param name="id">ID de la tarjeta</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConId(Guid id)
    {
        _id = id;
        _logger.LogDebug("ID de tarjeta establecido: {Id}", id);
        return this;
    }

    /// <summary>
    /// Establece la tarjeta como activa
    /// </summary>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConEstadoActivo()
    {
        _estadoActivo = true;
        _activa = true;
        _logger.LogDebug("Estado activo establecido");
        return this;
    }

    /// <summary>
    /// Establece la tarjeta como inactiva
    /// </summary>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConEstadoInactivo()
    {
        _estadoActivo = false;
        _activa = false;
        _logger.LogDebug("Estado inactivo establecido");
        return this;
    }

    /// <summary>
    /// Establece los puntos disponibles en la tarjeta
    /// </summary>
    /// <param name="puntos">Puntos disponibles</param>
    /// <returns>Builder para encadenamiento fluido</returns>
    public TarjetaFidelizacionBuilder ConPuntosDisponibles(int puntos)
    {
        _puntosDisponibles = puntos;
        _logger.LogDebug("Puntos disponibles establecidos: {Puntos}", puntos);
        return this;
    }

    /// <summary>
    /// Construye la tarjeta de fidelización
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tarjeta de fidelización creada</returns>
    public async Task<Result<TarjetaFidelizacion>> ConstruirAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Iniciando construcción de tarjeta de fidelización");

        // Validar configuración antes de construir
        var validacion = ValidarConfiguracion();
        if (!validacion.Succeeded)
        {
            return Result.Failure<TarjetaFidelizacion>(validacion.Error ?? "Error de validación");
        }

        try
        {
            // Crear la tarjeta usando el método estático de la entidad
            var tarjeta = TarjetaFidelizacion.Crear(
                _clienteId!.Value,
                _numeroTarjeta!);

            // Configurar nivel específico si se especificó
            if (_nivel.HasValue && _nivel.Value != NivelFidelizacion.Basico)
            {
                tarjeta.ActualizarNivel(_nivel.Value);
            }

            // Configurar fecha de vencimiento si se especificó
            if (_fechaVencimiento.HasValue)
            {
                // TODO: Implementar método para actualizar fecha de vencimiento
                // tarjeta.ActualizarFechaExpiracion(_fechaVencimiento.Value);
            }

            // Configurar propiedades opcionales
            if (_puntosIniciales.HasValue && _puntosIniciales.Value > 0)
            {
                var historialPuntos = tarjeta.AgregarPuntos(_puntosIniciales.Value, "Puntos iniciales");
            }

            // Activar la tarjeta si está configurada como activa
            if (_activa)
            {
                tarjeta.Activar();
            }

            _logger.LogInformation("Tarjeta de fidelización construida exitosamente: {TarjetaId}", tarjeta.Id);
            return Result.Success(tarjeta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error construyendo tarjeta de fidelización");
            return Result.Failure<TarjetaFidelizacion>($"Error construyendo tarjeta: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida la configuración antes de construir
    /// </summary>
    /// <returns>Resultado de la validación</returns>
    public Result ValidarConfiguracion()
    {
        if (!_clienteId.HasValue || _clienteId.Value == Guid.Empty)
        {
            _notificationManager.AddError("El cliente es requerido para crear la tarjeta", "ClienteId");
        }

        if (string.IsNullOrWhiteSpace(_numeroTarjeta))
        {
            _notificationManager.AddError("El número de tarjeta es requerido", "NumeroTarjeta");
        }

        if (!_nivel.HasValue)
        {
            _notificationManager.AddError("El nivel de fidelización es requerido", "Nivel");
        }

        if (_notificationManager.HasErrors)
        {
            var errores = string.Join("; ", _notificationManager.GetErrors().Select(e => e.Message));
            return Result.Failure($"Errores de validación: {errores}");
        }

        return Result.Success();
    }

    /// <summary>
    /// Resetea el builder para crear una nueva tarjeta
    /// </summary>
    /// <returns>Builder reseteado</returns>
    public TarjetaFidelizacionBuilder Reset()
    {
        _clienteId = null;
        _tipoTarjeta = null;
        _numeroTarjeta = null;
        _puntosIniciales = null;
        _nivel = null;
        _fechaVencimiento = null;
        _activa = true;
        _beneficios.Clear();
        _multiplicador = 1.0m;
        _observaciones = null;
        _datosAdicionales = null;
        _incluirCodigoQR = false;

        _notificationManager.ClearErrors();
        _logger.LogDebug("Builder reseteado para nueva construcción");
        return this;
    }

    /// <summary>
    /// Método Build para compatibilidad con tests (sinónimo de ConstruirAsync)
    /// </summary>
    /// <returns>Tarjeta de fidelización creada</returns>
    public TarjetaFidelizacion Build()
    {
        var result = ConstruirAsync().GetAwaiter().GetResult();
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Error construyendo tarjeta: {result.Error}");
        }
        return result.Value;
    }

    /// <summary>
    /// Implementación simple de INotificationManager para tests
    /// </summary>
    private class SimpleNotificationManager : INotificationManager
    {
        private readonly Notification _notification = new();

        public INotification CurrentNotification => _notification;
        public bool HasErrors => _notification.HasErrors;

        public ReadOnlyCollection<Error> GetErrors()
        {
            return _notification.Errors;
        }

        public void AddError(string errorMessage, string? errorCode = null, string? propertyName = null)
        {
            _notification.AddError(errorMessage, errorCode, propertyName);
        }

        public void AddErrors(IEnumerable<Error> errors)
        {
            _notification.AddErrors(errors);
        }

        public void AddErrors(INotification notification)
        {
            _notification.AddErrors(notification);
        }

        public void AddErrorsFromResult(Result result)
        {
            if (!result.Succeeded && result.Errors != null)
            {
                foreach (var error in result.Errors)
                {
                    _notification.AddError(error);
                }
            }
        }

        public void ClearErrors()
        {
            _notification.ClearErrors();
        }

        public INotification CreateNewNotification()
        {
            return new Notification();
        }

        public Result ToResult()
        {
            return _notification.ToResult();
        }

        public Result<T> ToResult<T>(T value)
        {
            return _notification.ToResult(value);
        }

        public void AddInformation(string message, string? code = null)
        {
            // Implementación vacía para compatibilidad
        }
    }

    /// <summary>
    /// Implementación simple de ILogger para tests
    /// </summary>
    private class SimpleLogger : ILogger<TarjetaFidelizacionBuilder>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Implementación vacía para tests
        }
    }
} 