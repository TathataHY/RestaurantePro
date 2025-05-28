namespace RestaurantePro.Domain.Core.SharedKernel.Validation;
using System.Diagnostics;

/// <summary>
/// Implementación del gestor central de notificaciones de la aplicación
/// </summary>
public class NotificationManager : INotificationManager
{
    private INotification _currentNotification;
    
    /// <summary>
    /// Obtiene la notificación actual
    /// </summary>
    public INotification CurrentNotification => _currentNotification;
    
    /// <summary>
    /// Indica si hay errores en la notificación actual
    /// </summary>
    public bool HasErrors => _currentNotification.HasErrors;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public NotificationManager()
    {
        _currentNotification = new Notification();
    }
    
    /// <summary>
    /// Obtiene una colección de solo lectura con los errores actuales
    /// </summary>
    public ReadOnlyCollection<Error> GetErrors()
    {
        return _currentNotification.Errors;
    }
    
    /// <summary>
    /// Añade un error a la notificación actual
    /// </summary>
    public void AddError(string errorMessage, string? errorCode = null, string? propertyName = null)
    {
        _currentNotification.AddError(errorMessage, errorCode, propertyName);
    }
    
    /// <summary>
    /// Añade múltiples errores a la notificación actual
    /// </summary>
    public void AddErrors(IEnumerable<Error> errors)
    {
        _currentNotification.AddErrors(errors);
    }
    
    /// <summary>
    /// Añade los errores de otra notificación
    /// </summary>
    public void AddErrors(INotification notification)
    {
        _currentNotification.AddErrors(notification);
    }
    
    /// <summary>
    /// Añade los errores de un Result fallido
    /// </summary>
    public void AddErrorsFromResult(Result result)
    {
        if (!result.Succeeded)
        {
            if (!string.IsNullOrEmpty(result.Error))
            {
                AddError(result.Error);
            }
            
            if (result.Errors != null && result.Errors.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    if (!string.IsNullOrEmpty(error))
                    {
                        AddError(error);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Limpia todos los errores de la notificación actual
    /// </summary>
    public void ClearErrors()
    {
        _currentNotification.ClearErrors();
    }
    
    /// <summary>
    /// Crea una nueva notificación y la establece como actual
    /// </summary>
    public INotification CreateNewNotification()
    {
        _currentNotification = new Notification();
        return _currentNotification;
    }
    
    /// <summary>
    /// Convierte la notificación actual en un Result
    /// </summary>
    public Result ToResult()
    {
        return _currentNotification.ToResult();
    }
    
    /// <summary>
    /// Convierte la notificación actual en un Result<T> con el valor proporcionado
    /// </summary>
    public Result<T> ToResult<T>(T value)
    {
        return _currentNotification.ToResult(value);
    }
    
    /// <summary>
    /// Añade un mensaje informativo a la notificación actual
    /// </summary>
    /// <param name="message">Mensaje informativo</param>
    /// <param name="code">Código del mensaje (opcional)</param>
    public void AddInformation(string message, string? code = null)
    {
        // En esta implementación simple, solo guardamos la información en el log
        // En una implementación más completa, podríamos guardar los mensajes informativos
        // en la notificación actual
        Debug.WriteLine($"INFO: [{code ?? "INFO"}] {message}");
    }
} 