namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Wrapper para respuestas de la API - Compatible con Backend
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa (compatible con backend)
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Datos de respuesta
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Mensaje informativo
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista de errores (si los hay)
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Código de estado HTTP
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? string.Empty,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> ErrorResponse(List<string> errors, string? message = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = message ?? string.Empty,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Crea una respuesta de error con un solo mensaje
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string error, string? message = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error },
            Message = message ?? string.Empty,
            StatusCode = statusCode
        };
    }
} 