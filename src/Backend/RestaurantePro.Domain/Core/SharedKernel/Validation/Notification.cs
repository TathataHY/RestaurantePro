namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Implementación del patrón Notification para validaciones y manejo de errores
/// </summary>
public class Notification : INotification
{
    private readonly List<Error> _errors;
    
    /// <summary>
    /// Colección de errores de solo lectura
    /// </summary>
    public ReadOnlyCollection<Error> Errors => _errors.AsReadOnly();
    
    /// <summary>
    /// Indica si la notificación tiene errores
    /// </summary>
    public bool HasErrors => _errors.Count > 0;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public Notification()
    {
        _errors = new List<Error>();
    }
    
    /// <summary>
    /// Añade un error a la notificación
    /// </summary>
    /// <param name="errorMessage">Mensaje de error</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    public void AddError(string errorMessage, string? errorCode = null, string? propertyName = null)
    {
        _errors.Add(new Error(errorMessage, errorCode, propertyName));
    }
    
    /// <summary>
    /// Añade múltiples errores a la notificación
    /// </summary>
    /// <param name="errors">Lista de errores a añadir</param>
    public void AddErrors(IEnumerable<Error> errors)
    {
        _errors.AddRange(errors);
    }
    
    /// <summary>
    /// Añade múltiples errores a partir de otra notificación
    /// </summary>
    /// <param name="notification">Notificación de la que extraer los errores</param>
    public void AddErrors(INotification notification)
    {
        _errors.AddRange(notification.Errors);
    }
    
    /// <summary>
    /// Limpia todos los errores de la notificación
    /// </summary>
    public void ClearErrors()
    {
        _errors.Clear();
    }
    
    /// <summary>
    /// Convierte la notificación en un Result. Si no hay errores, retorna un éxito.
    /// Si hay errores, retorna un fallo con los mensajes de error.
    /// </summary>
    /// <returns>Resultado que representa el estado de la notificación</returns>
    public Result ToResult()
    {
        if (!HasErrors)
            return Result.Success();
        
        var errorMessages = _errors.Select(e => e.Message).ToList();
        return Result.Failure(errorMessages);
    }
    
    /// <summary>
    /// Convierte la notificación en un Result<T>. Si no hay errores, retorna un éxito con el valor proporcionado.
    /// Si hay errores, retorna un fallo con los mensajes de error.
    /// </summary>
    /// <typeparam name="T">Tipo del valor de retorno</typeparam>
    /// <param name="value">Valor a incluir en el resultado exitoso</param>
    /// <returns>Resultado que representa el estado de la notificación con un valor</returns>
    public Result<T> ToResult<T>(T value)
    {
        if (!HasErrors)
            return Result.Success(value);
        
        var errorMessages = _errors.Select(e => e.Message).ToList();
        return Result.Failure<T>(errorMessages);
    }
} 