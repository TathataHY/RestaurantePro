namespace RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Builders
{
    /// <summary>
    /// Builder para la construcción fluida y validada de entidades Mesa.
    /// 
    /// Proporciona una interfaz fluida para construir mesas con todas las validaciones
    /// de negocio aplicadas de manera consistente y expresiva.
    /// 
    /// Características:
    /// - Validaciones robustas de reglas de negocio
    /// - Integración con INotificationManager
    /// - Patrón Result para manejo de errores
    /// - Interfaz fluida expresiva
    /// - Método Reset() para reutilización
    /// </summary>
    public class MesaBuilder
    {
        private readonly INotificationManager _notificationManager;
        private readonly ILogger<MesaBuilder> _logger;

        // Propiedades internas para construcción
        private int? _numero;
        private int? _capacidad;
        private string? _ubicacion;

        public MesaBuilder(INotificationManager notificationManager, ILogger<MesaBuilder> logger)
        {
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Establece el número de la mesa.
        /// </summary>
        /// <param name="numero">Número de la mesa (debe ser mayor que 0)</param>
        /// <returns>Instancia del builder para encadenamiento fluido</returns>
        public MesaBuilder ConNumero(int numero)
        {
            _logger.LogDebug("Estableciendo número de mesa: {Numero}", numero);

            if (numero <= 0)
            {
                _notificationManager.AddError("El número de mesa debe ser mayor que 0", "Numero");
                return this;
            }

            _numero = numero;
            return this;
        }

        /// <summary>
        /// Establece la capacidad de la mesa.
        /// </summary>
        /// <param name="capacidad">Capacidad de la mesa (debe ser mayor que 0)</param>
        /// <returns>Instancia del builder para encadenamiento fluido</returns>
        public MesaBuilder ConCapacidad(int capacidad)
        {
            _logger.LogDebug("Estableciendo capacidad de mesa: {Capacidad}", capacidad);

            if (capacidad <= 0)
            {
                _notificationManager.AddError("La capacidad debe ser mayor que 0", "Capacidad");
                return this;
            }

            if (capacidad > 50) // Límite razonable para una mesa
            {
                _notificationManager.AddError("La capacidad no puede ser mayor a 50 personas", "Capacidad");
                return this;
            }

            _capacidad = capacidad;
            return this;
        }

        /// <summary>
        /// Establece la ubicación de la mesa.
        /// </summary>
        /// <param name="ubicacion">Ubicación de la mesa (no puede estar vacía)</param>
        /// <returns>Instancia del builder para encadenamiento fluido</returns>
        public MesaBuilder EnUbicacion(string ubicacion)
        {
            _logger.LogDebug("Estableciendo ubicación de mesa: {Ubicacion}", ubicacion);

            if (string.IsNullOrWhiteSpace(ubicacion))
            {
                _notificationManager.AddError("La ubicación no puede estar vacía", "Ubicacion");
                return this;
            }

            if (ubicacion.Length > 100)
            {
                _notificationManager.AddError("La ubicación no puede tener más de 100 caracteres", "Ubicacion");
                return this;
            }

            _ubicacion = ubicacion.Trim();
            return this;
        }

        /// <summary>
        /// Construye la entidad Mesa con todas las validaciones aplicadas.
        /// </summary>
        /// <returns>Result con la Mesa construida o errores de validación</returns>
        public Result<Mesa> Construir()
        {
            _logger.LogDebug("Iniciando construcción de Mesa");

            // NO limpiar notificaciones si ya hay errores acumulados
            if (!_notificationManager.HasErrors)
            {
                _notificationManager.CreateNewNotification();
            }

            // Validar campos obligatorios
            if (!_numero.HasValue)
            {
                _notificationManager.AddError("El número de mesa es obligatorio", "Numero");
            }

            if (!_capacidad.HasValue)
            {
                _notificationManager.AddError("La capacidad es obligatoria", "Capacidad");
            }

            if (string.IsNullOrWhiteSpace(_ubicacion))
            {
                _notificationManager.AddError("La ubicación es obligatoria", "Ubicacion");
            }

            // Verificar si hay errores de validación
            if (_notificationManager.HasErrors)
            {
                var errores = string.Join(", ", _notificationManager.GetErrors().Select(n => n.Message));
                _logger.LogWarning("Error en construcción de Mesa: {Errores}", errores);
                return _notificationManager.ToResult<Mesa>(null!);
            }

            try
            {
                // Construir la entidad usando el método de fábrica
                var mesa = Mesa.Crear(_numero!.Value, _capacidad!.Value, _ubicacion!);

                _logger.LogInformation(
                    "Mesa construida exitosamente - Número: {Numero}, Capacidad: {Capacidad}, Ubicación: {Ubicacion}",
                    mesa.Numero, mesa.Capacidad, mesa.Ubicacion);

                return Result.Success(mesa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al construir Mesa");
                _notificationManager.AddError($"Error al construir la mesa: {ex.Message}", "Construccion");
                return _notificationManager.ToResult<Mesa>(null!);
            }
        }

        /// <summary>
        /// Reinicia el builder para permitir su reutilización.
        /// </summary>
        /// <returns>Instancia del builder limpia</returns>
        public MesaBuilder Reset()
        {
            _logger.LogDebug("Reiniciando MesaBuilder");

            _numero = null;
            _capacidad = null;
            _ubicacion = null;

            _notificationManager.CreateNewNotification();

            return this;
        }
    }
} 