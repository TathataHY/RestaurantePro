using RestaurantePro.Mobile.Core.Models.Enums;

namespace RestaurantePro.Mobile.Core.Services.Navigation;

/// <summary>
/// Servicio de navegación con verificación de autorización integrada
/// Valida permisos antes de permitir navegación a páginas restringidas
/// </summary>
public interface IAuthorizedNavigationService : INavigationService
{
    /// <summary>
    /// Navega a una ruta verificando primero los permisos requeridos
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="requiredPermission">Permiso requerido para acceder</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithPermissionAsync(string route, AppPermission requiredPermission, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta verificando primero los permisos requeridos (requiere al menos uno)
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="requiredPermissions">Permisos requeridos (al menos uno)</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithAnyPermissionAsync(string route, AppPermission[] requiredPermissions, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta verificando primero la funcionalidad requerida
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="requiredFeature">Funcionalidad requerida para acceder</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithFeatureAsync(string route, AppFeature requiredFeature, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta verificando primero el rol requerido
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="requiredRole">Rol requerido para acceder</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithRoleAsync(string route, string requiredRole, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta verificando primero los roles requeridos (requiere al menos uno)
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="requiredRoles">Roles requeridos (al menos uno)</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithAnyRoleAsync(string route, string[] requiredRoles, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta con parámetros verificando primero los permisos requeridos
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="parameters">Parámetros de navegación</param>
    /// <param name="requiredPermission">Permiso requerido para acceder</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithPermissionAsync(string route, IDictionary<string, object> parameters, AppPermission requiredPermission, bool showDeniedMessage = true);
    
    /// <summary>
    /// Navega a una ruta con parámetros verificando primero la funcionalidad requerida
    /// </summary>
    /// <param name="route">Ruta de destino</param>
    /// <param name="parameters">Parámetros de navegación</param>
    /// <param name="requiredFeature">Funcionalidad requerida para acceder</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    /// <returns>True si navegó exitosamente, False si se denegó el acceso</returns>
    Task<bool> NavigateToWithFeatureAsync(string route, IDictionary<string, object> parameters, AppFeature requiredFeature, bool showDeniedMessage = true);
}
