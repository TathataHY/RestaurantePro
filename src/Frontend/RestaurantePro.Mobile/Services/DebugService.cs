using Microsoft.Maui.Controls;

namespace RestaurantePro.Mobile.Services;

/// <summary>
/// Servicio para mostrar información de debugging en la aplicación móvil
/// </summary>
public static class DebugService
{
    /// <summary>
    /// Muestra un popup de debug con información detallada
    /// </summary>
    public static void ShowDebugPopup(string title, string message)
    {
#if DEBUG
        try
        {
            Application.Current?.Dispatcher.Dispatch(async () =>
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(title, message, "OK");
                }
            });
        }
        catch (Exception ex)
        {
            // Si falla el popup, al menos escribir en consola
            System.Diagnostics.Debug.WriteLine($"Error al mostrar popup: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// Muestra información del HttpClient
    /// </summary>
    public static void ShowHttpClientInfo(string baseAddress, string timeout, string headers)
    {
        var message = $"🔧 HttpClient Config:\nBaseAddress: {baseAddress}\nTimeout: {timeout}\nHeaders: {headers}";
        ShowDebugPopup("🐛 HttpClient Debug", message);
    }

    /// <summary>
    /// Muestra información de la petición HTTP
    /// </summary>
    public static void ShowHttpRequest(string method, string url, string data)
    {
        var message = $"🚀 {method} Request\nURL: {url}\nData: {data}";
        ShowDebugPopup("🐛 HTTP Request", message);
    }

    /// <summary>
    /// Muestra información de la respuesta HTTP
    /// </summary>
    public static void ShowHttpResponse(string statusCode, string content)
    {
        var message = $"📥 Respuesta recibida\nStatus: {statusCode}\nContent: {content}";
        ShowDebugPopup("🐛 HTTP Response", message);
    }

    /// <summary>
    /// Muestra información de excepción
    /// </summary>
    public static void ShowException(string operation, Exception ex)
    {
        var message = $"💥 EXCEPCIÓN en {operation}:\nTipo: {ex.GetType().Name}\nMensaje: {ex.Message}\nStackTrace: {ex.StackTrace}";
        ShowDebugPopup("🐛 Exception Debug", message);
    }

    /// <summary>
    /// Muestra información del login
    /// </summary>
    public static void ShowLoginInfo(string email, string password)
    {
        var message = $"🔐 Iniciando Login\nEmail: {email}\nPassword: {new string('*', password.Length)}";
        ShowDebugPopup("🐛 Login Debug", message);
    }

    /// <summary>
    /// Muestra información del endpoint
    /// </summary>
    public static void ShowEndpointInfo(string endpoint)
    {
        var message = $"📡 Llamando endpoint: {endpoint}";
        ShowDebugPopup("🐛 Endpoint Debug", message);
    }

    /// <summary>
    /// Muestra información de la respuesta de la API
    /// </summary>
    public static void ShowApiResponseInfo(bool success, string message, List<string> errors)
    {
        var errorText = errors != null ? string.Join(", ", errors) : "Ninguno";
        var responseInfo = $"📥 Respuesta API:\nSuccess: {success}\nMessage: {message}\nErrors: {errorText}";
        ShowDebugPopup("🐛 API Response Debug", responseInfo);
    }
}
