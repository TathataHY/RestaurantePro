namespace RestaurantePro.Mobile.Core.Core.Attributes;

/// <summary>
/// Atributo para decorar ViewModels y comandos que requieren roles específicos
/// Utilizado para validación automática de autorización basada en roles
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute
{
    /// <summary>
    /// Roles requeridos para acceder al elemento decorado
    /// </summary>
    public string[] RequiredRoles { get; }
    
    /// <summary>
    /// Indica si se requieren TODOS los roles o solo UNO de ellos
    /// Por defecto: false (solo requiere uno de los roles)
    /// </summary>
    public bool RequireAllRoles { get; set; } = false;
    
    /// <summary>
    /// Mensaje de error personalizado cuando no se tienen los roles
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Constructor para un solo rol
    /// </summary>
    /// <param name="role">Rol requerido</param>
    public RequireRoleAttribute(string role)
    {
        RequiredRoles = new[] { role };
    }

    /// <summary>
    /// Constructor para múltiples roles
    /// </summary>
    /// <param name="roles">Roles requeridos</param>
    public RequireRoleAttribute(params string[] roles)
    {
        RequiredRoles = roles ?? throw new ArgumentNullException(nameof(roles));
        
        if (roles.Length == 0)
        {
            throw new ArgumentException("Debe especificar al menos un rol", nameof(roles));
        }
    }
}
