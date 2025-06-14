using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.Logging.Configuration
{
    /// <summary>
    /// Configuración para los servicios de logging
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// Nivel mínimo de log global
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";
        
        /// <summary>
        /// Indica si se debe usar enriquecimiento estructurado
        /// </summary>
        public bool UseEnrichers { get; set; } = true;
        
        /// <summary>
        /// Indica si se debe escribir en consola
        /// </summary>
        public bool WriteToConsole { get; set; } = true;
        
        /// <summary>
        /// Indica si se debe escribir en archivo
        /// </summary>
        public bool WriteToFile { get; set; } = true;
        
        /// <summary>
        /// Ruta del archivo de log
        /// </summary>
        public string FilePath { get; set; } = "logs/restaurante-pro-.log";
        
        /// <summary>
        /// Política de retención de archivos en días
        /// </summary>
        public int RetainedFileCountLimit { get; set; } = 31;
        
        /// <summary>
        /// Tamaño máximo del archivo en bytes antes de rotar
        /// </summary>
        public long FileSizeLimitBytes { get; set; } = 10485760; // 10 MB
        
        /// <summary>
        /// Niveles mínimos por namespace
        /// </summary>
        public Dictionary<string, string> OverridesByNamespace { get; set; } = new Dictionary<string, string>
        {
            { "Microsoft", "Warning" },
            { "System", "Warning" },
            { "Microsoft.AspNetCore", "Warning" },
            { "Microsoft.EntityFrameworkCore", "Warning" },
            { "RestaurantePro.Infrastructure", "Information" },
            { "RestaurantePro.API", "Information" }
        };
        
        /// <summary>
        /// Configuración para Application Insights
        /// </summary>
        public AppInsightsConfiguration ApplicationInsights { get; set; } = new AppInsightsConfiguration();
    }
    
    /// <summary>
    /// Configuración para Application Insights
    /// </summary>
    public class AppInsightsConfiguration
    {
        /// <summary>
        /// Indica si Application Insights está habilitado
        /// </summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// Clave de instrumentación de Application Insights
        /// </summary>
        public string InstrumentationKey { get; set; } = "";
    }
} 