namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Extensiones para el patrón Notification
/// </summary>
public static class NotificationExtensions
{
    /// <summary>
    /// Verifica una condición y añade un error si no se cumple
    /// </summary>
    /// <param name="notification">Notificación</param>
    /// <param name="condition">Condición a verificar</param>
    /// <param name="errorMessage">Mensaje de error si la condición no se cumple</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>La notificación actualizada</returns>
    public static INotification Require(
        this INotification notification, 
        bool condition, 
        string errorMessage, 
        string? errorCode = null, 
        string? propertyName = null)
    {
        if (!condition)
        {
            notification.AddError(errorMessage, errorCode, propertyName);
        }
        
        return notification;
    }
    
    /// <summary>
    /// Verifica que un objeto no sea nulo y añade un error si lo es
    /// </summary>
    /// <param name="notification">Notificación</param>
    /// <param name="value">Valor a verificar</param>
    /// <param name="errorMessage">Mensaje de error si el valor es nulo</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>La notificación actualizada</returns>
    public static INotification RequireNotNull(
        this INotification notification, 
        object? value, 
        string errorMessage, 
        string? errorCode = null, 
        string? propertyName = null)
    {
        return notification.Require(value != null, errorMessage, errorCode, propertyName);
    }
    
    /// <summary>
    /// Verifica que una cadena no sea nula ni vacía y añade un error si lo es
    /// </summary>
    /// <param name="notification">Notificación</param>
    /// <param name="value">Valor a verificar</param>
    /// <param name="errorMessage">Mensaje de error si el valor es nulo o vacío</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>La notificación actualizada</returns>
    public static INotification RequireNotEmpty(
        this INotification notification, 
        string? value, 
        string errorMessage, 
        string? errorCode = null, 
        string? propertyName = null)
    {
        return notification.Require(!string.IsNullOrEmpty(value), errorMessage, errorCode, propertyName);
    }
    
    /// <summary>
    /// Ejecuta una función si la notificación no tiene errores
    /// </summary>
    /// <param name="notification">Notificación</param>
    /// <param name="action">Acción a ejecutar</param>
    /// <returns>La notificación actualizada</returns>
    public static INotification OnSuccess(
        this INotification notification, 
        Action action)
    {
        if (!notification.HasErrors)
        {
            action();
        }
        
        return notification;
    }
    
    /// <summary>
    /// Encadena dos validaciones para un mismo objeto
    /// </summary>
    /// <typeparam name="T">Tipo del objeto a validar</typeparam>
    /// <param name="notification">Notificación</param>
    /// <param name="value">Valor a validar</param>
    /// <param name="validationFunc">Función de validación a aplicar</param>
    /// <returns>La notificación actualizada</returns>
    public static INotification ValidateAnd<T>(
        this INotification notification, 
        T value, 
        Action<INotification, T> validationFunc) where T : class
    {
        validationFunc(notification, value);
        return notification;
    }
} 