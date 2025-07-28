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
    /// Propiedad de conveniencia para compatibilidad con ViewModels
    /// </summary>
    public bool Succeeded => Success;
    
    /// <summary>
    /// Propiedad de conveniencia para compatibilidad con ViewModels
    /// </summary>
    public string Error => Errors.FirstOrDefault() ?? string.Empty;
    
    /// <summary>
    /// Código de estado HTTP
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operación exitosa")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 200,
            Errors = new List<string>()
        };
    }
    
    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> ErrorResponse(List<string> errors, string message = "Error en la operación", int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }
    
    /// <summary>
    /// Crea una respuesta de error con un solo mensaje
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string error, string message = "Error en la operación", int statusCode = 400)
    {
        return ErrorResponse(new List<string> { error }, message, statusCode);
    }
    
    /// <summary>
    /// Método de conveniencia para compatibilidad con servicios
    /// </summary>
    public static ApiResponse<T> Failure(string error, string message = "Error en la operación", int statusCode = 400)
    {
        return ErrorResponse(error, message, statusCode);
    }
    
    /// <summary>
    /// Método de conveniencia para compatibilidad con servicios
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string message = "Operación exitosa")
    {
        return SuccessResponse(data, message);
    }
} 