namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción base para errores de aplicación
/// Representa errores que ocurren en la capa de aplicación
/// </summary>
public class AppException : Exception
{
    /// <summary>
    /// Código de error específico para categorización
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Detalles adicionales sobre el error
    /// </summary>
    public object? Details { get; }

    /// <summary>
    /// Severidad del error
    /// </summary>
    public ErrorSeverity Severity { get; }

    public AppException() : base() 
    {
        Severity = ErrorSeverity.Medium;
    }

    public AppException(string message) : base(message) 
    {
        Severity = ErrorSeverity.Medium;
    }

    public AppException(string message, Exception innerException)
        : base(message, innerException) 
    {
        Severity = ErrorSeverity.High;
    }

    public AppException(string message, string errorCode, ErrorSeverity severity = ErrorSeverity.Medium)
        : base(message)
    {
        ErrorCode = errorCode;
        Severity = severity;
    }

    public AppException(string message, string errorCode, object details, ErrorSeverity severity = ErrorSeverity.Medium)
        : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
        Severity = severity;
    }

    public AppException(string message, Exception innerException, string errorCode, ErrorSeverity severity = ErrorSeverity.High)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Severity = severity;
    }

    /// <summary>
    /// Crea una AppException para errores de configuración
    /// </summary>
    public static AppException ForConfiguration(string message, string configKey)
    {
        return new AppException(
            $"Error de configuración: {message}",
            "CONFIGURATION_ERROR",
            new { ConfigurationKey = configKey },
            ErrorSeverity.Critical);
    }

    /// <summary>
    /// Crea una AppException para errores de servicios externos
    /// </summary>
    public static AppException ForExternalService(string serviceName, string message, Exception? innerException = null)
    {
        return new AppException(
            $"Error en servicio externo '{serviceName}': {message}",
            innerException ?? new Exception(message),
            "EXTERNAL_SERVICE_ERROR",
            ErrorSeverity.High);
    }

    /// <summary>
    /// Crea una AppException para errores de datos
    /// </summary>
    public static AppException ForDataAccess(string operation, string message, Exception? innerException = null)
    {
        return new AppException(
            $"Error de acceso a datos en operación '{operation}': {message}",
            innerException ?? new Exception(message),
            "DATA_ACCESS_ERROR",
            ErrorSeverity.High);
    }

    /// <summary>
    /// Crea una AppException para errores de negocio
    /// </summary>
    public static AppException ForBusinessLogic(string operation, string message)
    {
        return new AppException(
            $"Error de lógica de negocio en '{operation}': {message}",
            "BUSINESS_LOGIC_ERROR",
            new { Operation = operation },
            ErrorSeverity.Medium);
    }
}

/// <summary>
/// Severidad de los errores para categorización y logging
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Error informativo, no requiere acción inmediata
    /// </summary>
    Low = 1,

    /// <summary>
    /// Error de severidad media, requiere atención
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Error de alta severidad, requiere acción inmediata
    /// </summary>
    High = 3,

    /// <summary>
    /// Error crítico, puede afectar la operación del sistema
    /// </summary>
    Critical = 4
} 