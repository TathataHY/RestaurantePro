namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Representa un error de validación o de dominio
/// </summary>
public class Error
{
    /// <summary>
    /// Mensaje de error
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Código de error (opcional)
    /// </summary>
    public string? Code { get; }
    
    /// <summary>
    /// Nombre de la propiedad que generó el error (opcional)
    /// </summary>
    public string? PropertyName { get; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Mensaje de error</param>
    /// <param name="code">Código de error (opcional)</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    public Error(string message, string? code = null, string? propertyName = null)
    {
        Message = message;
        Code = code;
        PropertyName = propertyName;
    }
} 