using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.Caching.Configuration
{
    /// <summary>
    /// Configuración para los servicios de caché
    /// </summary>
    public class CacheConfiguration
    {
        /// <summary>
        /// Indica si el caché está habilitado
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Tiempo de expiración predeterminado en minutos
        /// </summary>
        public int DefaultExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Prefijo para todas las claves de caché
        /// </summary>
        public string KeyPrefix { get; set; } = "RestaurantePro:";

        /// <summary>
        /// Configuración específica para Redis
        /// </summary>
        public RedisConfiguration Redis { get; set; } = new RedisConfiguration();

        /// <summary>
        /// Configuración de políticas de caché para diferentes tipos de datos
        /// </summary>
        public Dictionary<string, CachePolicy> Policies { get; set; } = new Dictionary<string, CachePolicy>
        {
            { "Productos", new CachePolicy { ExpirationMinutes = 60 } },
            { "Clientes", new CachePolicy { ExpirationMinutes = 120 } },
            { "Reservaciones", new CachePolicy { ExpirationMinutes = 15 } },
            { "Mesas", new CachePolicy { ExpirationMinutes = 10 } },
            { "Ingredientes", new CachePolicy { ExpirationMinutes = 30 } },
            { "Proveedores", new CachePolicy { ExpirationMinutes = 240 } }
        };
    }

    /// <summary>
    /// Configuración específica para Redis
    /// </summary>
    public class RedisConfiguration
    {
        /// <summary>
        /// Cadena de conexión a Redis
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        public string InstanceName { get; set; } = "RestaurantePro";

        /// <summary>
        /// Número de base de datos
        /// </summary>
        public int Database { get; set; } = 0;

        /// <summary>
        /// Tiempo de espera de conexión en milisegundos
        /// </summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>
        /// Tiempo de espera de sincronización en milisegundos
        /// </summary>
        public int SyncTimeout { get; set; } = 5000;

        /// <summary>
        /// Indica si se debe usar SSL
        /// </summary>
        public bool UseSsl { get; set; } = false;
    }

    /// <summary>
    /// Política de caché para un tipo específico de datos
    /// </summary>
    public class CachePolicy
    {
        /// <summary>
        /// Tiempo de expiración en minutos
        /// </summary>
        public int ExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Indica si se debe usar caché distribuido para este tipo de datos
        /// </summary>
        public bool UseDistributedCache { get; set; } = true;

        /// <summary>
        /// Prioridad de caché
        /// </summary>
        public CachePriority Priority { get; set; } = CachePriority.Normal;
    }

    /// <summary>
    /// Prioridad de caché
    /// </summary>
    public enum CachePriority
    {
        /// <summary>
        /// Baja prioridad
        /// </summary>
        Low = 0,

        /// <summary>
        /// Prioridad normal
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Alta prioridad
        /// </summary>
        High = 2,

        /// <summary>
        /// Prioridad crítica
        /// </summary>
        Critical = 3
    }
} 