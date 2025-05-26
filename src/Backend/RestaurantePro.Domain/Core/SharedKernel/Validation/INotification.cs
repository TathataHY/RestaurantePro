namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Interfaz para el patrón Notification para validaciones y manejo de errores
/// </summary>
public interface INotification
{
    /// <summary>
    /// Colección de errores de solo lectura
    /// </summary>
    ReadOnlyCollection<Error> Errors { get; }
    
    /// <summary>
    /// Indica si la notificación tiene errores
    /// </summary>
    bool HasErrors { get; }
    
    /// <summary>
    /// Añade un error a la notificación
    /// </summary>
    void AddError(string errorMessage, string? errorCode = null, string? propertyName = null);
    
    /// <summary>
    /// Añade múltiples errores a la notificación
    /// </summary>
    void AddErrors(IEnumerable<Error> errors);
    
    /// <summary>
    /// Añade múltiples errores a partir de otra notificación
    /// </summary>
    void AddErrors(INotification notification);
    
    /// <summary>
    /// Limpia todos los errores de la notificación
    /// </summary>
    void ClearErrors();
    
    /// <summary>
    /// Convierte la notificación en un Result. Si no hay errores, retorna un éxito.
    /// Si hay errores, retorna un fallo con los mensajes de error.
    /// </summary>
    Result ToResult();
    
    /// <summary>
    /// Convierte la notificación en un Result<T>. Si no hay errores, retorna un éxito con el valor proporcionado.
    /// Si hay errores, retorna un fallo con los mensajes de error.
    /// </summary>
    Result<T> ToResult<T>(T value);
} 