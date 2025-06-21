using System.Collections.Generic;

namespace RestaurantePro.Api.Common
{
    /// <summary>
    /// Clase para estandarizar todas las respuestas de la API
    /// </summary>
    /// <typeparam name="T">Tipo de datos a devolver</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica si la operación fue exitosa
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
        public static ApiResponse<T> SuccessResponse(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }
        
        /// <summary>
        /// Crea una respuesta de error
        /// </summary>
        public static ApiResponse<T> ErrorResponse(List<string> errors, string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
                Message = message,
                StatusCode = statusCode
            };
        }
        
        /// <summary>
        /// Crea una respuesta de error con un solo mensaje
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string error, string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Message = message,
                StatusCode = statusCode
            };
        }
    }
} 