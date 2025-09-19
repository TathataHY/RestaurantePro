using RestaurantePro.Mobile.Core.Models.Enums;

namespace RestaurantePro.Mobile.Core.Core.Attributes;

/// <summary>
/// Atributo para decorar ViewModels y comandos que requieren permisos específicos
/// Utilizado para validación automática de autorización
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute
{
    /// <summary>
    /// Permisos requeridos para acceder al elemento decorado
    /// </summary>
    public AppPermission[] RequiredPermissions { get; }
    
    /// <summary>
    /// Indica si se requieren TODOS los permisos o solo UNO de ellos
    /// Por defecto: false (solo requiere uno de los permisos)
    /// </summary>
    public bool RequireAllPermissions { get; set; } = false;
    
    /// <summary>
    /// Mensaje de error personalizado cuando no se tienen los permisos
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Constructor para un solo permiso
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    public RequirePermissionAttribute(AppPermission permission)
    {
        RequiredPermissions = new[] { permission };
    }

    /// <summary>
    /// Constructor para múltiples permisos
    /// </summary>
    /// <param name="permissions">Permisos requeridos</param>
    public RequirePermissionAttribute(params AppPermission[] permissions)
    {
        RequiredPermissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        
        if (permissions.Length == 0)
        {
            throw new ArgumentException("Debe especificar al menos un permiso", nameof(permissions));
        }
    }
}
