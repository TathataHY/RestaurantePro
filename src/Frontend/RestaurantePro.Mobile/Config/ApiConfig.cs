namespace RestaurantePro.Mobile.Config;

/// <summary>
/// Configuración para la API del backend
/// </summary>
public static class ApiConfig
{
    /// <summary>
    /// URL base del backend (local)
    /// </summary>
    public const string BaseUrl = "http://192.168.8.101:5243/";
    
    /// <summary>
    /// Timeout para las peticiones HTTP
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    
    // En entorno local NO se requiere autenticación básica de hosting
    public static class HostingCredentials
    {
        public const string Username = "";
        public const string Password = "";
        public static string GetEncodedCredentials() => string.Empty;
    }
} 