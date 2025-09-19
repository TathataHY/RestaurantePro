using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Enums;

namespace RestaurantePro.Mobile.Core.Services.Authorization;

/// <summary>
/// Servicio de autorización para controlar acceso basado en roles
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Verifica si el usuario actual tiene un permiso específico
    /// </summary>
    /// <param name="permission">Permiso a verificar</param>
    /// <returns>True si tiene el permiso, False en caso contrario</returns>
    Task<bool> HasPermissionAsync(AppPermission permission);
    
    /// <summary>
    /// Verifica si el usuario actual tiene alguno de los permisos especificados
    /// </summary>
    /// <param name="permissions">Lista de permisos a verificar</param>
    /// <returns>True si tiene al menos uno de los permisos, False en caso contrario</returns>
    Task<bool> HasAnyPermissionAsync(params AppPermission[] permissions);
    
    /// <summary>
    /// Verifica si el usuario actual tiene todos los permisos especificados
    /// </summary>
    /// <param name="permissions">Lista de permisos a verificar</param>
    /// <returns>True si tiene todos los permisos, False en caso contrario</returns>
    Task<bool> HasAllPermissionsAsync(params AppPermission[] permissions);
    
    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <returns>True si tiene el rol, False en caso contrario</returns>
    Task<bool> HasRoleAsync(string role);
    
    /// <summary>
    /// Obtiene todos los permisos del usuario actual
    /// </summary>
    /// <returns>Lista de permisos del usuario</returns>
    Task<List<AppPermission>> GetUserPermissionsAsync();
    
    /// <summary>
    /// Obtiene los roles del usuario actual
    /// </summary>
    /// <returns>Lista de roles del usuario</returns>
    Task<List<string>> GetUserRolesAsync();
    
    /// <summary>
    /// Verifica si el usuario puede acceder a una funcionalidad específica
    /// </summary>
    /// <param name="feature">Funcionalidad a verificar</param>
    /// <returns>True si puede acceder, False en caso contrario</returns>
    Task<bool> CanAccessFeatureAsync(AppFeature feature);
}
