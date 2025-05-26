namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Interfaz para el gestor central de notificaciones de la aplicación
/// </summary>
public interface INotificationManager
{
    /// <summary>
    /// Obtiene la notificación actual
    /// </summary>
    INotification CurrentNotification { get; }
    
    /// <summary>
    /// Indica si hay errores en la notificación actual
    /// </summary>
    bool HasErrors { get; }
    
    /// <summary>
    /// Obtiene una colección de solo lectura con los errores actuales
    /// </summary>
    ReadOnlyCollection<Error> GetErrors();
    
    /// <summary>
    /// Añade un error a la notificación actual
    /// </summary>
    /// <param name="errorMessage">Mensaje de error</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    void AddError(string errorMessage, string? errorCode = null, string? propertyName = null);
    
    /// <summary>
    /// Añade múltiples errores a la notificación actual
    /// </summary>
    /// <param name="errors">Errores a añadir</param>
    void AddErrors(IEnumerable<Error> errors);
    
    /// <summary>
    /// Añade los errores de otra notificación
    /// </summary>
    /// <param name="notification">Notificación de origen</param>
    void AddErrors(INotification notification);
    
    /// <summary>
    /// Añade los errores de un Result fallido
    /// </summary>
    /// <param name="result">Resultado fallido</param>
    void AddErrorsFromResult(Result result);
    
    /// <summary>
    /// Limpia todos los errores de la notificación actual
    /// </summary>
    void ClearErrors();
    
    /// <summary>
    /// Crea una nueva notificación y la establece como actual
    /// </summary>
    /// <returns>La nueva notificación creada</returns>
    INotification CreateNewNotification();
    
    /// <summary>
    /// Convierte la notificación actual en un Result
    /// </summary>
    /// <returns>Un Result exitoso si no hay errores, o fallido con los errores actuales</returns>
    Result ToResult();
    
    /// <summary>
    /// Convierte la notificación actual en un Result<T> con el valor proporcionado
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="value">Valor a incluir en el resultado</param>
    /// <returns>Un Result<T> exitoso con el valor si no hay errores, o fallido con los errores actuales</returns>
    Result<T> ToResult<T>(T value);
} 