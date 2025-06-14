namespace RestaurantePro.Infrastructure.Logging.Configuration
{
    /// <summary>
    /// Políticas para los servicios de logging
    /// </summary>
    public static class LoggingPolicies
    {
        /// <summary>
        /// Niveles de log disponibles
        /// </summary>
        public static class LogLevels
        {
            public const string Verbose = "Verbose";
            public const string Debug = "Debug";
            public const string Information = "Information";
            public const string Warning = "Warning";
            public const string Error = "Error";
            public const string Fatal = "Fatal";
        }
        
        /// <summary>
        /// Plantillas de mensajes predefinidas
        /// </summary>
        public static class MessageTemplates
        {
            /// <summary>
            /// Plantilla para logs de API
            /// </summary>
            public const string ApiTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
            
            /// <summary>
            /// Plantilla para logs de consola
            /// </summary>
            public const string ConsoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <{SourceContext}>{NewLine}{Exception}";
            
            /// <summary>
            /// Plantilla para logs con información de usuario
            /// </summary>
            public const string UserAwareTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj} [User:{UserId}] [Correlation:{CorrelationId}]{NewLine}{Exception}";
        }
        
        /// <summary>
        /// Tiempo máximo para mantener los logs en diversas fuentes
        /// </summary>
        public static class RetentionPolicies
        {
            /// <summary>
            /// Tiempo de retención de logs en archivos (días)
            /// </summary>
            public const int FileLogRetentionDays = 30;
            
            /// <summary>
            /// Tiempo de retención de logs en base de datos (días)
            /// </summary>
            public const int DatabaseLogRetentionDays = 90;
        }
    }
} 