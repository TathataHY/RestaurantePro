using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Core.Helpers;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;
using RestaurantePro.Mobile.Core.Services.Dialog;

namespace RestaurantePro.Mobile.Core.Models.ViewModels;

/// <summary>
/// ViewModel base con funcionalidades de autorización integradas
/// Proporciona métodos para verificar permisos y controlar acceso
/// </summary>
public abstract partial class AuthorizedBaseViewModel : BaseViewModel
{
    protected readonly IAuthorizationService AuthorizationService;
    protected readonly IAuthorizationValidator AuthorizationValidator;
    protected readonly AuthorizationUIHelper UIHelper;
    protected readonly IDialogService DialogService;

    [ObservableProperty]
    private bool isAuthorized = true;

    [ObservableProperty]
    private string authorizationMessage = string.Empty;

    protected AuthorizedBaseViewModel(
        IAuthorizationService authorizationService,
        IAuthorizationValidator authorizationValidator,
        AuthorizationUIHelper uiHelper,
        IDialogService dialogService,
        ILogger logger) : base(logger)
    {
        AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        AuthorizationValidator = authorizationValidator ?? throw new ArgumentNullException(nameof(authorizationValidator));
        UIHelper = uiHelper ?? throw new ArgumentNullException(nameof(uiHelper));
        DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>
    /// Valida los permisos del ViewModel basado en sus atributos de autorización
    /// </summary>
    protected virtual async Task ValidateAuthorizationAsync()
    {
        try
        {
            var result = await AuthorizationValidator.ValidateClassAsync(GetType());
            
            IsAuthorized = result.IsAllowed;
            AuthorizationMessage = result.ErrorMessage ?? string.Empty;

            if (!IsAuthorized)
            {
                Logger.LogWarning("Usuario no autorizado para acceder a {ViewModelName}: {Message}", 
                    GetType().Name, AuthorizationMessage);
                
                await HandleAuthorizationFailedAsync(AuthorizationMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validando autorización en {ViewModelName}", GetType().Name);
            IsAuthorized = false;
            AuthorizationMessage = "Error interno de autorización";
            await HandleAuthorizationFailedAsync("No se pudo verificar los permisos");
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene un permiso específico
    /// </summary>
    /// <param name="permission">Permiso a verificar</param>
    /// <returns>True si tiene el permiso, False en caso contrario</returns>
    protected async Task<bool> HasPermissionAsync(AppPermission permission)
    {
        return await AuthorizationService.HasPermissionAsync(permission);
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a una funcionalidad
    /// </summary>
    /// <param name="feature">Funcionalidad a verificar</param>
    /// <returns>True si puede acceder, False en caso contrario</returns>
    protected async Task<bool> CanAccessFeatureAsync(AppFeature feature)
    {
        return await AuthorizationService.CanAccessFeatureAsync(feature);
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <returns>True si tiene el rol, False en caso contrario</returns>
    protected async Task<bool> HasRoleAsync(string role)
    {
        return await AuthorizationService.HasRoleAsync(role);
    }

    /// <summary>
    /// Ejecuta una acción solo si el usuario tiene el permiso requerido
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    /// <param name="action">Acción a ejecutar</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    protected async Task ExecuteIfAuthorizedAsync(AppPermission permission, Func<Task> action, bool showDeniedMessage = true)
    {
        try
        {
            if (await HasPermissionAsync(permission))
            {
                await action();
            }
            else if (showDeniedMessage)
            {
                var message = UIHelper.GetAccessDeniedMessage(permission);
                await DialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error ejecutando acción con autorización para permiso {Permission}", permission);
            await ShowErrorAsync("Error interno ejecutando la acción");
        }
    }

    /// <summary>
    /// Ejecuta una acción solo si el usuario puede acceder a la funcionalidad
    /// </summary>
    /// <param name="feature">Funcionalidad requerida</param>
    /// <param name="action">Acción a ejecutar</param>
    /// <param name="showDeniedMessage">Si mostrar mensaje cuando se deniegue el acceso</param>
    protected async Task ExecuteIfAuthorizedAsync(AppFeature feature, Func<Task> action, bool showDeniedMessage = true)
    {
        try
        {
            if (await CanAccessFeatureAsync(feature))
            {
                await action();
            }
            else if (showDeniedMessage)
            {
                var message = UIHelper.GetAccessDeniedMessage(feature);
                await DialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error ejecutando acción con autorización para funcionalidad {Feature}", feature);
            await ShowErrorAsync("Error interno ejecutando la acción");
        }
    }

    /// <summary>
    /// Obtiene la visibilidad de un elemento UI basado en un permiso
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    protected async Task<bool> GetVisibilityAsync(AppPermission permission)
    {
        return await UIHelper.IsVisibleAsync(permission);
    }

    /// <summary>
    /// Obtiene la visibilidad de un elemento UI basado en una funcionalidad
    /// </summary>
    /// <param name="feature">Funcionalidad requerida</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    protected async Task<bool> GetVisibilityAsync(AppFeature feature)
    {
        return await UIHelper.IsVisibleAsync(feature);
    }

    /// <summary>
    /// Maneja el caso cuando la autorización falla
    /// Puede ser sobrescrito por ViewModels derivados para comportamiento personalizado
    /// </summary>
    /// <param name="message">Mensaje de error</param>
    protected virtual async Task HandleAuthorizationFailedAsync(string message)
    {
        await DialogService.ShowAlertAsync("Acceso Denegado", message, "OK");
    }

    /// <summary>
    /// Verifica autorización al inicializar el ViewModel
    /// Debe llamarse en OnAppearing o método similar
    /// </summary>
    public virtual async Task InitializeWithAuthorizationAsync()
    {
        await ValidateAuthorizationAsync();
        
        if (IsAuthorized)
        {
            await OnAuthorizedInitializeAsync();
        }
    }

    /// <summary>
    /// Método llamado después de validar autorización exitosamente
    /// Debe ser implementado por ViewModels derivados para inicialización específica
    /// </summary>
    protected virtual async Task OnAuthorizedInitializeAsync()
    {
        // Implementación por defecto vacía
        await Task.CompletedTask;
    }
}
