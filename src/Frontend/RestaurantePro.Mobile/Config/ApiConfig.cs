using System.Text;

namespace RestaurantePro.Mobile.Config;

/// <summary>
/// Configuración para la API del backend
/// </summary>
public static class ApiConfig
{
    /// <summary>
    /// URL base del backend (BETA)
    /// </summary>
    public const string BaseUrl = "http://limoncitoydedos-001-site1.jtempurl.com/";
    
    /// <summary>
    /// Timeout para las peticiones HTTP
    /// </summary>
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);
    
    // En entorno local NO se requiere autenticación básica de hosting
    public static class HostingCredentials
    {
        // Credenciales de protección de la URL temporal del hosting
        public const string Username = "11263519";
        public const string Password = "60-dayfreetrial";
        public static string GetEncodedCredentials()
        {
            var raw = $"{Username}:{Password}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
        }
    }
} 