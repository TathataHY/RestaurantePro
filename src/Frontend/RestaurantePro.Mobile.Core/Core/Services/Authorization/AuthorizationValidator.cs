using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Core.Attributes;
using RestaurantePro.Mobile.Core.Models.Enums;
using System.Reflection;

namespace RestaurantePro.Mobile.Core.Services.Authorization;

/// <summary>
/// Validador de autorización que verifica atributos de autorización en ViewModels y métodos
/// </summary>
public class AuthorizationValidator : IAuthorizationValidator
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationValidator> _logger;

    public AuthorizationValidator(IAuthorizationService authorizationService, ILogger<AuthorizationValidator> logger)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Valida si el usuario actual puede acceder a una clase decorada con atributos de autorización
    /// </summary>
    /// <param name="type">Tipo de la clase a validar</param>
    /// <returns>Resultado de la validación</returns>
    public async Task<AuthorizationResult> ValidateClassAsync(Type type)
    {
        try
        {
            // Verificar atributos de funcionalidad
            var featureAttribute = type.GetCustomAttribute<RequireFeatureAttribute>();
            if (featureAttribute != null)
            {
                var canAccess = await _authorizationService.CanAccessFeatureAsync(featureAttribute.RequiredFeature);
                if (!canAccess)
                {
                    var message = featureAttribute.ErrorMessage ?? 
                                $"No tiene acceso a la funcionalidad: {featureAttribute.RequiredFeature}";
                    return AuthorizationResult.Denied(message);
                }
            }

            // Verificar atributos de permisos
            var permissionAttribute = type.GetCustomAttribute<RequirePermissionAttribute>();
            if (permissionAttribute != null)
            {
                var hasPermission = permissionAttribute.RequireAllPermissions
                    ? await _authorizationService.HasAllPermissionsAsync(permissionAttribute.RequiredPermissions)
                    : await _authorizationService.HasAnyPermissionAsync(permissionAttribute.RequiredPermissions);

                if (!hasPermission)
                {
                    var message = permissionAttribute.ErrorMessage ?? 
                                $"No tiene los permisos requeridos: {string.Join(", ", permissionAttribute.RequiredPermissions)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            // Verificar atributos de roles
            var roleAttribute = type.GetCustomAttribute<RequireRoleAttribute>();
            if (roleAttribute != null)
            {
                var hasRole = roleAttribute.RequireAllRoles
                    ? await ValidateAllRolesAsync(roleAttribute.RequiredRoles)
                    : await ValidateAnyRoleAsync(roleAttribute.RequiredRoles);

                if (!hasRole)
                {
                    var message = roleAttribute.ErrorMessage ?? 
                                $"No tiene los roles requeridos: {string.Join(", ", roleAttribute.RequiredRoles)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            return AuthorizationResult.Allowed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando autorización para clase {ClassName}", type.Name);
            return AuthorizationResult.Denied("Error interno de autorización");
        }
    }

    /// <summary>
    /// Valida si el usuario actual puede ejecutar un método decorado con atributos de autorización
    /// </summary>
    /// <param name="method">Información del método a validar</param>
    /// <returns>Resultado de la validación</returns>
    public async Task<AuthorizationResult> ValidateMethodAsync(MethodInfo method)
    {
        try
        {
            // Verificar atributos de funcionalidad
            var featureAttribute = method.GetCustomAttribute<RequireFeatureAttribute>();
            if (featureAttribute != null)
            {
                var canAccess = await _authorizationService.CanAccessFeatureAsync(featureAttribute.RequiredFeature);
                if (!canAccess)
                {
                    var message = featureAttribute.ErrorMessage ?? 
                                $"No tiene acceso a la funcionalidad: {featureAttribute.RequiredFeature}";
                    return AuthorizationResult.Denied(message);
                }
            }

            // Verificar atributos de permisos
            var permissionAttribute = method.GetCustomAttribute<RequirePermissionAttribute>();
            if (permissionAttribute != null)
            {
                var hasPermission = permissionAttribute.RequireAllPermissions
                    ? await _authorizationService.HasAllPermissionsAsync(permissionAttribute.RequiredPermissions)
                    : await _authorizationService.HasAnyPermissionAsync(permissionAttribute.RequiredPermissions);

                if (!hasPermission)
                {
                    var message = permissionAttribute.ErrorMessage ?? 
                                $"No tiene los permisos requeridos: {string.Join(", ", permissionAttribute.RequiredPermissions)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            // Verificar atributos de roles
            var roleAttribute = method.GetCustomAttribute<RequireRoleAttribute>();
            if (roleAttribute != null)
            {
                var hasRole = roleAttribute.RequireAllRoles
                    ? await ValidateAllRolesAsync(roleAttribute.RequiredRoles)
                    : await ValidateAnyRoleAsync(roleAttribute.RequiredRoles);

                if (!hasRole)
                {
                    var message = roleAttribute.ErrorMessage ?? 
                                $"No tiene los roles requeridos: {string.Join(", ", roleAttribute.RequiredRoles)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            return AuthorizationResult.Allowed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando autorización para método {MethodName}", method.Name);
            return AuthorizationResult.Denied("Error interno de autorización");
        }
    }

    /// <summary>
    /// Valida si el usuario actual puede acceder a una propiedad decorada con atributos de autorización
    /// </summary>
    /// <param name="property">Información de la propiedad a validar</param>
    /// <returns>Resultado de la validación</returns>
    public async Task<AuthorizationResult> ValidatePropertyAsync(PropertyInfo property)
    {
        try
        {
            // Verificar atributos de permisos
            var permissionAttribute = property.GetCustomAttribute<RequirePermissionAttribute>();
            if (permissionAttribute != null)
            {
                var hasPermission = permissionAttribute.RequireAllPermissions
                    ? await _authorizationService.HasAllPermissionsAsync(permissionAttribute.RequiredPermissions)
                    : await _authorizationService.HasAnyPermissionAsync(permissionAttribute.RequiredPermissions);

                if (!hasPermission)
                {
                    var message = permissionAttribute.ErrorMessage ?? 
                                $"No tiene los permisos requeridos: {string.Join(", ", permissionAttribute.RequiredPermissions)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            // Verificar atributos de roles
            var roleAttribute = property.GetCustomAttribute<RequireRoleAttribute>();
            if (roleAttribute != null)
            {
                var hasRole = roleAttribute.RequireAllRoles
                    ? await ValidateAllRolesAsync(roleAttribute.RequiredRoles)
                    : await ValidateAnyRoleAsync(roleAttribute.RequiredRoles);

                if (!hasRole)
                {
                    var message = roleAttribute.ErrorMessage ?? 
                                $"No tiene los roles requeridos: {string.Join(", ", roleAttribute.RequiredRoles)}";
                    return AuthorizationResult.Denied(message);
                }
            }

            return AuthorizationResult.Allowed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validando autorización para propiedad {PropertyName}", property.Name);
            return AuthorizationResult.Denied("Error interno de autorización");
        }
    }

    private async Task<bool> ValidateAllRolesAsync(string[] roles)
    {
        foreach (var role in roles)
        {
            if (!await _authorizationService.HasRoleAsync(role))
            {
                return false;
            }
        }
        return true;
    }

    private async Task<bool> ValidateAnyRoleAsync(string[] roles)
    {
        foreach (var role in roles)
        {
            if (await _authorizationService.HasRoleAsync(role))
            {
                return true;
            }
        }
        return false;
    }
}
