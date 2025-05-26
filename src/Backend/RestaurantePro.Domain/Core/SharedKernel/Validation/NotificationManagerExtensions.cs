namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Extensiones para el gestor de notificaciones
/// </summary>
public static class NotificationManagerExtensions
{
    /// <summary>
    /// Verifica una condición y añade un error si no se cumple
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="condition">Condición a verificar</param>
    /// <param name="errorMessage">Mensaje de error si la condición no se cumple</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager Require(
        this INotificationManager manager,
        bool condition,
        string errorMessage,
        string? errorCode = null,
        string? propertyName = null)
    {
        if (!condition)
        {
            manager.AddError(errorMessage, errorCode, propertyName);
        }
        
        return manager;
    }
    
    /// <summary>
    /// Verifica que un objeto no sea nulo y añade un error si lo es
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="value">Valor a verificar</param>
    /// <param name="errorMessage">Mensaje de error si el valor es nulo</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager RequireNotNull(
        this INotificationManager manager,
        object? value,
        string errorMessage,
        string? errorCode = null,
        string? propertyName = null)
    {
        return manager.Require(value != null, errorMessage, errorCode, propertyName);
    }
    
    /// <summary>
    /// Verifica que una cadena no sea nula ni vacía y añade un error si lo es
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="value">Valor a verificar</param>
    /// <param name="errorMessage">Mensaje de error si el valor es nulo o vacío</param>
    /// <param name="errorCode">Código de error opcional</param>
    /// <param name="propertyName">Nombre de la propiedad que generó el error (opcional)</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager RequireNotEmpty(
        this INotificationManager manager,
        string? value,
        string errorMessage,
        string? errorCode = null,
        string? propertyName = null)
    {
        return manager.Require(!string.IsNullOrEmpty(value), errorMessage, errorCode, propertyName);
    }
    
    /// <summary>
    /// Verifica que un Result sea exitoso y añade sus errores si no lo es
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="result">Resultado a verificar</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager RequireSuccess(
        this INotificationManager manager,
        Result result)
    {
        if (!result.Succeeded)
        {
            manager.AddErrorsFromResult(result);
        }
        
        return manager;
    }
    
    /// <summary>
    /// Ejecuta una acción solo si no hay errores en la notificación actual
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="action">Acción a ejecutar</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager OnSuccess(
        this INotificationManager manager,
        Action action)
    {
        if (!manager.HasErrors)
        {
            action();
        }
        
        return manager;
    }
    
    /// <summary>
    /// Ejecuta una función que devuelve un Result solo si no hay errores en la notificación actual
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="func">Función a ejecutar</param>
    /// <returns>El gestor de notificaciones</returns>
    public static INotificationManager OnSuccess(
        this INotificationManager manager,
        Func<Result> func)
    {
        if (!manager.HasErrors)
        {
            var result = func();
            if (!result.Succeeded)
            {
                manager.AddErrorsFromResult(result);
            }
        }
        
        return manager;
    }
    
    /// <summary>
    /// Ejecuta una función asíncrona que devuelve un Result solo si no hay errores en la notificación actual
    /// </summary>
    /// <param name="manager">Gestor de notificaciones</param>
    /// <param name="func">Función asíncrona a ejecutar</param>
    /// <returns>Tarea que representa la operación asíncrona</returns>
    public static async Task<INotificationManager> OnSuccessAsync(
        this INotificationManager manager,
        Func<Task<Result>> func)
    {
        if (!manager.HasErrors)
        {
            var result = await func();
            if (!result.Succeeded)
            {
                manager.AddErrorsFromResult(result);
            }
        }
        
        return manager;
    }
} 