using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Services.Dialog;

namespace RestaurantePro.Mobile.Core.Services.Navigation;

/// <summary>
/// Implementación del servicio de navegación con autorización integrada
/// Verifica permisos antes de permitir navegación a páginas restringidas
/// </summary>
public class AuthorizedNavigationService : IAuthorizedNavigationService
{
    private readonly INavigationService _navigationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly AuthorizationUIHelper _uiHelper;
    private readonly IDialogService _dialogService;
    private readonly ILogger<AuthorizedNavigationService> _logger;

    public AuthorizedNavigationService(
        INavigationService navigationService,
        IAuthorizationService authorizationService,
        AuthorizationUIHelper uiHelper,
        IDialogService dialogService,
        ILogger<AuthorizedNavigationService> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _uiHelper = uiHelper ?? throw new ArgumentNullException(nameof(uiHelper));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region INavigationService Implementation (delegados al servicio base)

    public async Task NavigateToAsync(string route)
    {
        await _navigationService.NavigateToAsync(route);
    }

    public async Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        await _navigationService.NavigateToAsync(route, parameters);
    }

    public async Task GoBackAsync()
    {
        await _navigationService.GoBackAsync();
    }

    public async Task GoToRootAsync()
    {
        await _navigationService.GoToRootAsync();
    }

    #endregion

    #region Navegación con Permisos

    public async Task<bool> NavigateToWithPermissionAsync(string route, AppPermission requiredPermission, bool showDeniedMessage = true)
    {
        try
        {
            var hasPermission = await _authorizationService.HasPermissionAsync(requiredPermission);
            if (hasPermission)
            {
                await _navigationService.NavigateToAsync(route);
                _logger.LogInformation("Navegación autorizada a {Route} con permiso {Permission}", route, requiredPermission);
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación denegada a {Route} - falta permiso {Permission}", route, requiredPermission);
                
                if (showDeniedMessage)
                {
                    var message = _uiHelper.GetAccessDeniedMessage(requiredPermission);
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización para navegación a {Route}", route);
            return false;
        }
    }

    public async Task<bool> NavigateToWithAnyPermissionAsync(string route, AppPermission[] requiredPermissions, bool showDeniedMessage = true)
    {
        try
        {
            var hasAnyPermission = await _authorizationService.HasAnyPermissionAsync(requiredPermissions);
            if (hasAnyPermission)
            {
                await _navigationService.NavigateToAsync(route);
                _logger.LogInformation("Navegación autorizada a {Route} con cualquiera de los permisos: {Permissions}", 
                    route, string.Join(", ", requiredPermissions));
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación denegada a {Route} - faltan permisos: {Permissions}", 
                    route, string.Join(", ", requiredPermissions));
                
                if (showDeniedMessage)
                {
                    var message = $"No tiene ninguno de los permisos requeridos: {string.Join(", ", requiredPermissions)}";
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización para navegación a {Route}", route);
            return false;
        }
    }

    public async Task<bool> NavigateToWithPermissionAsync(string route, IDictionary<string, object> parameters, AppPermission requiredPermission, bool showDeniedMessage = true)
    {
        try
        {
            var hasPermission = await _authorizationService.HasPermissionAsync(requiredPermission);
            if (hasPermission)
            {
                await _navigationService.NavigateToAsync(route, parameters);
                _logger.LogInformation("Navegación con parámetros autorizada a {Route} con permiso {Permission}", route, requiredPermission);
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación con parámetros denegada a {Route} - falta permiso {Permission}", route, requiredPermission);
                
                if (showDeniedMessage)
                {
                    var message = _uiHelper.GetAccessDeniedMessage(requiredPermission);
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización para navegación con parámetros a {Route}", route);
            return false;
        }
    }

    #endregion

    #region Navegación con Funcionalidades

    public async Task<bool> NavigateToWithFeatureAsync(string route, AppFeature requiredFeature, bool showDeniedMessage = true)
    {
        try
        {
            var canAccessFeature = await _authorizationService.CanAccessFeatureAsync(requiredFeature);
            if (canAccessFeature)
            {
                await _navigationService.NavigateToAsync(route);
                _logger.LogInformation("Navegación autorizada a {Route} con funcionalidad {Feature}", route, requiredFeature);
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación denegada a {Route} - no tiene acceso a funcionalidad {Feature}", route, requiredFeature);
                
                if (showDeniedMessage)
                {
                    var message = _uiHelper.GetAccessDeniedMessage(requiredFeature);
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización de funcionalidad para navegación a {Route}", route);
            return false;
        }
    }

    public async Task<bool> NavigateToWithFeatureAsync(string route, IDictionary<string, object> parameters, AppFeature requiredFeature, bool showDeniedMessage = true)
    {
        try
        {
            var canAccessFeature = await _authorizationService.CanAccessFeatureAsync(requiredFeature);
            if (canAccessFeature)
            {
                await _navigationService.NavigateToAsync(route, parameters);
                _logger.LogInformation("Navegación con parámetros autorizada a {Route} con funcionalidad {Feature}", route, requiredFeature);
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación con parámetros denegada a {Route} - no tiene acceso a funcionalidad {Feature}", route, requiredFeature);
                
                if (showDeniedMessage)
                {
                    var message = _uiHelper.GetAccessDeniedMessage(requiredFeature);
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización de funcionalidad para navegación con parámetros a {Route}", route);
            return false;
        }
    }

    #endregion

    #region Navegación con Roles

    public async Task<bool> NavigateToWithRoleAsync(string route, string requiredRole, bool showDeniedMessage = true)
    {
        try
        {
            var hasRole = await _authorizationService.HasRoleAsync(requiredRole);
            if (hasRole)
            {
                await _navigationService.NavigateToAsync(route);
                _logger.LogInformation("Navegación autorizada a {Route} con rol {Role}", route, requiredRole);
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación denegada a {Route} - no tiene rol {Role}", route, requiredRole);
                
                if (showDeniedMessage)
                {
                    var message = $"No tiene el rol requerido: {requiredRole}";
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización de rol para navegación a {Route}", route);
            return false;
        }
    }

    public async Task<bool> NavigateToWithAnyRoleAsync(string route, string[] requiredRoles, bool showDeniedMessage = true)
    {
        try
        {
            bool hasAnyRole = false;
            foreach (var role in requiredRoles)
            {
                if (await _authorizationService.HasRoleAsync(role))
                {
                    hasAnyRole = true;
                    break;
                }
            }

            if (hasAnyRole)
            {
                await _navigationService.NavigateToAsync(route);
                _logger.LogInformation("Navegación autorizada a {Route} con cualquiera de los roles: {Roles}", 
                    route, string.Join(", ", requiredRoles));
                return true;
            }
            else
            {
                _logger.LogWarning("Navegación denegada a {Route} - no tiene ninguno de los roles: {Roles}", 
                    route, string.Join(", ", requiredRoles));
                
                if (showDeniedMessage)
                {
                    var message = $"No tiene ninguno de los roles requeridos: {string.Join(", ", requiredRoles)}";
                    await _dialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando autorización de roles para navegación a {Route}", route);
            return false;
        }
    }

    #endregion
}
