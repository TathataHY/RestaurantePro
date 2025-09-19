using System.Reflection;

namespace RestaurantePro.Mobile.Core.Services.Authorization;

/// <summary>
/// Servicio de validación de autorización basado en atributos
/// </summary>
public interface IAuthorizationValidator
{
    /// <summary>
    /// Valida si el usuario actual puede acceder a una clase decorada con atributos de autorización
    /// </summary>
    /// <param name="type">Tipo de la clase a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<AuthorizationResult> ValidateClassAsync(Type type);
    
    /// <summary>
    /// Valida si el usuario actual puede ejecutar un método decorado con atributos de autorización
    /// </summary>
    /// <param name="method">Información del método a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<AuthorizationResult> ValidateMethodAsync(MethodInfo method);
    
    /// <summary>
    /// Valida si el usuario actual puede acceder a una propiedad decorada con atributos de autorización
    /// </summary>
    /// <param name="property">Información de la propiedad a validar</param>
    /// <returns>Resultado de la validación</returns>
    Task<AuthorizationResult> ValidatePropertyAsync(PropertyInfo property);
}
