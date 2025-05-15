using System;

namespace RestaurantePro.Mobile.Config
{
    /// <summary>
    /// Interfaz para la configuración de la aplicación
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// URL base de la API
        /// </summary>
        string ApiBaseUrl { get; }
        
        /// <summary>
        /// Versión de la API
        /// </summary>
        string ApiVersion { get; }
        
        /// <summary>
        /// Timeout para peticiones HTTP en segundos
        /// </summary>
        int ApiTimeoutSeconds { get; }
        
        /// <summary>
        /// Indica si la aplicación está en modo debug
        /// </summary>
        bool IsDebug { get; }
    }

    /// <summary>
    /// Implementación de la configuración de la aplicación
    /// </summary>
    public class AppSettings : IAppSettings
    {
        // En producción, usaríamos un servidor remoto
        private const string PRODUCTION_API_URL = "https://api.restaurantepro.com";
        
        // Para desarrollo, podemos usar localhost o un servidor de pruebas
        private const string DEVELOPMENT_API_URL = "https://10.0.2.2:5001"; // Android emulator localhost
        
        // Versión actual de la API
        private const string CURRENT_API_VERSION = "v1";
        
        // Timeout por defecto
        private const int DEFAULT_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// URL base de la API
        /// </summary>
        public string ApiBaseUrl 
        {
            get
            {
#if DEBUG
                return DEVELOPMENT_API_URL;
#else
                return PRODUCTION_API_URL;
#endif
            }
        }

        /// <summary>
        /// Versión de la API
        /// </summary>
        public string ApiVersion => CURRENT_API_VERSION;

        /// <summary>
        /// Timeout para peticiones HTTP en segundos
        /// </summary>
        public int ApiTimeoutSeconds => DEFAULT_TIMEOUT_SECONDS;

        /// <summary>
        /// Indica si la aplicación está en modo debug
        /// </summary>
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Construye la URL completa para un endpoint específico
        /// </summary>
        public string GetApiEndpoint(string endpointPath)
        {
            return $"{ApiBaseUrl}/api/{ApiVersion}/{endpointPath.TrimStart('/')}";
        }
    }
} 