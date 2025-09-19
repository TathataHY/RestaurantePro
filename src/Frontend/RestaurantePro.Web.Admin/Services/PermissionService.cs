using Microsoft.AspNetCore.Components.Authorization;
using RestaurantePro.Web.Admin.Auth;

namespace RestaurantePro.Web.Admin.Services;

public class PermissionService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly TokenStore _tokenStore;

    public PermissionService(AuthenticationStateProvider authStateProvider, TokenStore tokenStore)
    {
        _authStateProvider = authStateProvider;
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico
    /// </summary>
    public async Task<bool> HasRoleAsync(string role)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userRoles = _tokenStore.Roles ?? new List<string>();
        var identityRoles = authState.User.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var allUserRoles = userRoles.Union(identityRoles).ToList();
        return allUserRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifica si el usuario actual tiene alguno de los roles especificados
    /// </summary>
    public async Task<bool> HasAnyRoleAsync(params string[] roles)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userRoles = _tokenStore.Roles ?? new List<string>();
        var identityRoles = authState.User.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var allUserRoles = userRoles.Union(identityRoles).ToList();
        return roles.Any(role => allUserRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica si el usuario actual es Administrador
    /// </summary>
    public async Task<bool> IsAdministratorAsync()
    {
        return await HasRoleAsync("Administrador");
    }

    /// <summary>
    /// Verifica si el usuario actual es Gerente
    /// </summary>
    public async Task<bool> IsManagerAsync()
    {
        return await HasRoleAsync("Gerente");
    }

    /// <summary>
    /// Verifica si el usuario actual es Propietario
    /// </summary>
    public async Task<bool> IsOwnerAsync()
    {
        return await HasRoleAsync("Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede crear usuarios
    /// Administradores, Gerentes y Propietarios pueden crear usuarios
    /// </summary>
    public async Task<bool> CanCreateUsersAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede eliminar usuarios
    /// Solo Administradores pueden eliminar usuarios
    /// </summary>
    public async Task<bool> CanDeleteUsersAsync()
    {
        return await HasRoleAsync("Administrador");
    }

    /// <summary>
    /// Verifica si el usuario puede desactivar usuarios
    /// Solo Administradores pueden desactivar usuarios
    /// </summary>
    public async Task<bool> CanDeactivateUsersAsync()
    {
        return await HasRoleAsync("Administrador");
    }

    /// <summary>
    /// Verifica si el usuario puede editar usuarios
    /// Administradores, Gerentes y Propietarios pueden editar usuarios
    /// </summary>
    public async Task<bool> CanEditUsersAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede activar usuarios
    /// Administradores y Propietarios pueden activar usuarios
    /// </summary>
    public async Task<bool> CanActivateUsersAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede ver usuarios
    /// Todos los roles administrativos pueden ver usuarios
    /// </summary>
    public async Task<bool> CanViewUsersAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede crear productos
    /// Administradores, Gerentes y Propietarios pueden crear productos
    /// </summary>
    public async Task<bool> CanCreateProductsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede eliminar productos
    /// Solo Administradores y Propietarios pueden eliminar productos
    /// </summary>
    public async Task<bool> CanDeleteProductsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede editar productos
    /// Administradores, Gerentes y Propietarios pueden editar productos
    /// </summary>
    public async Task<bool> CanEditProductsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede ver productos
    /// Todos los roles administrativos pueden ver productos
    /// </summary>
    public async Task<bool> CanViewProductsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a configuración del sistema
    /// Solo Administradores y Propietarios pueden acceder a configuración
    /// </summary>
    public async Task<bool> CanAccessSystemConfigurationAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede ver reportes
    /// Todos los roles administrativos pueden ver reportes
    /// </summary>
    public async Task<bool> CanViewReportsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Verifica si el usuario puede exportar reportes
    /// Todos los roles administrativos pueden exportar reportes
    /// </summary>
    public async Task<bool> CanExportReportsAsync()
    {
        return await HasAnyRoleAsync("Administrador", "Gerente", "Propietario");
    }

    /// <summary>
    /// Obtiene el rol principal del usuario actual
    /// </summary>
    public async Task<string> GetUserRoleAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userRoles = _tokenStore.Roles ?? new List<string>();
        var identityRoles = authState.User.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        var allUserRoles = userRoles.Union(identityRoles).ToList();
        
        // Prioridad de roles: Administrador > Propietario > Gerente
        if (allUserRoles.Contains("Administrador", StringComparer.OrdinalIgnoreCase))
            return "Administrador";
        if (allUserRoles.Contains("Propietario", StringComparer.OrdinalIgnoreCase))
            return "Propietario";
        if (allUserRoles.Contains("Gerente", StringComparer.OrdinalIgnoreCase))
            return "Gerente";
        
        return allUserRoles.FirstOrDefault() ?? "Usuario";
    }

    // ===== PERMISOS DE DASHBOARD =====

    /// <summary>
    /// Verifica si el usuario puede ver datos financieros (ventas, ingresos, etc.)
    /// </summary>
    public async Task<bool> CanViewFinancialDataAsync()
    {
        var role = await GetUserRoleAsync();
        return role == "Administrador" || role == "Propietario";
    }

    /// <summary>
    /// Verifica si el usuario puede ver datos operativos (mesas, comandas, etc.)
    /// </summary>
    public async Task<bool> CanViewOperationalDataAsync()
    {
        var role = await GetUserRoleAsync();
        return role == "Administrador" || role == "Gerente" || role == "Propietario";
    }

    /// <summary>
    /// Verifica si el usuario puede ver reportes detallados y análisis avanzados
    /// </summary>
    public async Task<bool> CanViewDetailedReportsAsync()
    {
        var role = await GetUserRoleAsync();
        return role == "Administrador" || role == "Propietario";
    }
}
