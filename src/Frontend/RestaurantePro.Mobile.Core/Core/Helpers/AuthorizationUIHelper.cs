using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authorization;

namespace RestaurantePro.Mobile.Core.Core.Helpers;

/// <summary>
/// Helper para controlar elementos UI basado en autorización
/// Proporciona métodos para mostrar/ocultar elementos según permisos del usuario
/// </summary>
public class AuthorizationUIHelper
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationUIHelper> _logger;

    public AuthorizationUIHelper(IAuthorizationService authorizationService, ILogger<AuthorizationUIHelper> logger)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en un permiso
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleAsync(AppPermission permission)
    {
        try
        {
            return await _authorizationService.HasPermissionAsync(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para permiso {Permission}", permission);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en múltiples permisos (requiere al menos uno)
    /// </summary>
    /// <param name="permissions">Permisos requeridos</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleWithAnyPermissionAsync(params AppPermission[] permissions)
    {
        try
        {
            return await _authorizationService.HasAnyPermissionAsync(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para permisos múltiples: {Permissions}", string.Join(", ", permissions));
            return false;
        }
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en múltiples permisos (requiere todos)
    /// </summary>
    /// <param name="permissions">Permisos requeridos</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleWithAllPermissionsAsync(params AppPermission[] permissions)
    {
        try
        {
            return await _authorizationService.HasAllPermissionsAsync(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para todos los permisos: {Permissions}", string.Join(", ", permissions));
            return false;
        }
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en una funcionalidad
    /// </summary>
    /// <param name="feature">Funcionalidad requerida</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleAsync(AppFeature feature)
    {
        try
        {
            return await _authorizationService.CanAccessFeatureAsync(feature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para funcionalidad {Feature}", feature);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en un rol
    /// </summary>
    /// <param name="role">Rol requerido</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleForRoleAsync(string role)
    {
        try
        {
            return await _authorizationService.HasRoleAsync(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para rol {Role}", role);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un elemento UI debe ser visible basado en múltiples roles (requiere al menos uno)
    /// </summary>
    /// <param name="roles">Roles requeridos</param>
    /// <returns>True si debe ser visible, False en caso contrario</returns>
    public async Task<bool> IsVisibleForAnyRoleAsync(params string[] roles)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando visibilidad para roles múltiples: {Roles}", string.Join(", ", roles));
            return false;
        }
    }

    /// <summary>
    /// Verifica si un botón/comando debe estar habilitado basado en un permiso
    /// </summary>
    /// <param name="permission">Permiso requerido</param>
    /// <returns>True si debe estar habilitado, False en caso contrario</returns>
    public async Task<bool> IsEnabledAsync(AppPermission permission)
    {
        return await IsVisibleAsync(permission);
    }

    /// <summary>
    /// Verifica si un botón/comando debe estar habilitado basado en una funcionalidad
    /// </summary>
    /// <param name="feature">Funcionalidad requerida</param>
    /// <returns>True si debe estar habilitado, False en caso contrario</returns>
    public async Task<bool> IsEnabledAsync(AppFeature feature)
    {
        return await IsVisibleAsync(feature);
    }

    /// <summary>
    /// Obtiene un mensaje de "acceso denegado" personalizado para mostrar al usuario
    /// </summary>
    /// <param name="permission">Permiso que se intentó acceder</param>
    /// <returns>Mensaje de acceso denegado</returns>
    public string GetAccessDeniedMessage(AppPermission permission)
    {
        return permission switch
        {
            // Comandas
            AppPermission.CrearComandas => "No tiene permisos para crear comandas",
            AppPermission.ModificarComandas => "No tiene permisos para modificar comandas",
            AppPermission.CerrarComandas => "No tiene permisos para cerrar comandas",
            
            // Mesas
            AppPermission.CambiarEstadoMesas => "No tiene permisos para cambiar estados de mesas",
            AppPermission.AsignarMesas => "No tiene permisos para asignar mesas",
            
            // Preparaciones
            AppPermission.ActualizarEstadoPreparaciones => "No tiene permisos para actualizar preparaciones",
            AppPermission.CompletarPreparaciones => "No tiene permisos para completar preparaciones",
            
            // Facturación
            AppPermission.GenerarFacturas => "No tiene permisos para generar facturas",
            AppPermission.ProcesarPagos => "No tiene permisos para procesar pagos",
            
            // Supervisión
            AppPermission.SupervisarOperaciones => "No tiene permisos de supervisión",
            AppPermission.GestionarPersonalTurno => "No tiene permisos para gestionar personal",
            
            _ => $"No tiene permisos para realizar esta acción: {permission}"
        };
    }

    /// <summary>
    /// Obtiene un mensaje de "acceso denegado" personalizado para funcionalidades
    /// </summary>
    /// <param name="feature">Funcionalidad que se intentó acceder</param>
    /// <returns>Mensaje de acceso denegado</returns>
    public string GetAccessDeniedMessage(AppFeature feature)
    {
        return feature switch
        {
            AppFeature.GestionComandas => "No tiene acceso a la gestión de comandas",
            AppFeature.GestionMesas => "No tiene acceso a la gestión de mesas",
            AppFeature.Cocina => "No tiene acceso a las funciones de cocina",
            AppFeature.Caja => "No tiene acceso a las funciones de caja",
            AppFeature.Supervision => "No tiene acceso a las funciones de supervisión",
            AppFeature.Reservaciones => "No tiene acceso a la gestión de reservaciones",
            _ => $"No tiene acceso a esta funcionalidad: {feature}"
        };
    }
}
