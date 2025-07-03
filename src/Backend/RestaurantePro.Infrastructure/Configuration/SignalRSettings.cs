namespace RestaurantePro.Infrastructure.Configuration
{
    /// <summary>
    /// Configuración para SignalR
    /// </summary>
    public class SignalRSettings
    {
        /// <summary>
        /// Indica si SignalR está habilitado
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// URL del hub de notificaciones
        /// </summary>
        public string NotificationHubUrl { get; set; } = "/hubs/notifications";

        /// <summary>
        /// Timeout de conexión en segundos
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Máximo de conexiones concurrentes
        /// </summary>
        public int MaxConcurrentConnections { get; set; } = 1000;

        /// <summary>
        /// Habilitar errores detallados en desarrollo
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;

        /// <summary>
        /// String de conexión Redis para backplane (opcional)
        /// </summary>
        public string? BackplaneRedisConnectionString { get; set; }

        /// <summary>
        /// Intervalo de keep-alive en segundos
        /// </summary>
        public int KeepAliveIntervalSeconds { get; set; } = 15;

        /// <summary>
        /// Timeout del cliente en segundos
        /// </summary>
        public int ClientTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Tamaño máximo de mensaje recibido en bytes
        /// </summary>
        public int MaximumReceiveMessageSize { get; set; } = 32768;

        /// <summary>
        /// Capacidad del buffer de streams
        /// </summary>
        public int StreamBufferCapacity { get; set; } = 10;

        /// <summary>
        /// Habilitar logging detallado
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;

        /// <summary>
        /// Habilitar métricas de conexiones
        /// </summary>
        public bool EnableConnectionMetrics { get; set; } = true;

        /// <summary>
        /// Intervalo de limpieza de conexiones en segundos
        /// </summary>
        public int ConnectionCleanupIntervalSeconds { get; set; } = 300;
    }
} 