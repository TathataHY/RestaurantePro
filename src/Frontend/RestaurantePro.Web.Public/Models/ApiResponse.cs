namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// Respuesta estándar de la API (contenido en español, tipo en inglés).
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public int StatusCode { get; set; }
}


