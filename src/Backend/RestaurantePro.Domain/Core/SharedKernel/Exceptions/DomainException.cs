namespace RestaurantePro.Domain.Core.SharedKernel.Exceptions;

/// <summary>
/// Excepción base para todas las excepciones específicas del dominio de RestaurantePro.
/// Proporciona información contextual rica sobre errores de dominio.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Código de error específico del dominio
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Contexto de dominio donde ocurrió la excepción
    /// </summary>
    public string DomainContext { get; }
    
    /// <summary>
    /// Datos adicionales relacionados con la excepción
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; }
    
    /// <summary>
    /// Timestamp cuando ocurrió la excepción
    /// </summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Constructor base para excepciones de dominio
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error</param>
    /// <param name="errorCode">Código específico del error</param>
    /// <param name="domainContext">Contexto del dominio (ej: "Comercial", "Inventario")</param>
    /// <param name="innerException">Excepción interna si existe</param>
    protected DomainException(
        string message, 
        string errorCode, 
        string domainContext, 
        Exception? innerException = null) : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        DomainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        AdditionalData = new Dictionary<string, object>();
        OccurredAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Agrega datos contextuales a la excepción
    /// </summary>
    /// <param name="key">Clave del dato</param>
    /// <param name="value">Valor del dato</param>
    /// <returns>La misma instancia para encadenamiento fluido</returns>
    public DomainException WithData(string key, object value)
    {
        AdditionalData[key] = value;
        return this;
    }

    /// <summary>
    /// Agrega múltiples datos contextuales
    /// </summary>
    /// <param name="additionalData">Diccionario con datos adicionales</param>
    /// <returns>La misma instancia para encadenamiento fluido</returns>
    public DomainException WithData(Dictionary<string, object> additionalData)
    {
        foreach (var kvp in additionalData)
        {
            AdditionalData[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Convierte la excepción a una representación de Result para integración con el patrón Result
    /// </summary>
    /// <returns>Result con el error encapsulado</returns>
    public Result ToResult()
    {
        var errorMessage = $"[{ErrorCode}] {Message}";
        return Result.Failure(errorMessage);
    }

    /// <summary>
    /// Convierte la excepción a una representación de Result<T> para integración con el patrón Result
    /// </summary>
    /// <typeparam name="T">Tipo del valor de retorno</typeparam>
    /// <returns>Result<T> con el error encapsulado</returns>
    public Result<T> ToResult<T>()
    {
        var errorMessage = $"[{ErrorCode}] {Message}";
        return Result.Failure<T>(errorMessage);
    }

    /// <summary>
    /// Obtiene una representación detallada del error incluyendo contexto y datos
    /// </summary>
    /// <returns>String con información completa de la excepción</returns>
    public string GetDetailedMessage()
    {
        var details = new StringBuilder();
        details.AppendLine($"Error: [{ErrorCode}] {Message}");
        details.AppendLine($"Contexto: {DomainContext}");
        details.AppendLine($"Ocurrió en: {OccurredAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        if (AdditionalData.Any())
        {
            details.AppendLine("Datos adicionales:");
            foreach (var kvp in AdditionalData)
            {
                details.AppendLine($"  - {kvp.Key}: {kvp.Value}");
            }
        }
        
        if (InnerException != null)
        {
            details.AppendLine($"Excepción interna: {InnerException.Message}");
        }
        
        return details.ToString();
    }

    /// <summary>
    /// Override de ToString para incluir información adicional
    /// </summary>
    public override string ToString()
    {
        return GetDetailedMessage();
    }
} 