using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.Core.Models.Enums;
using RestaurantePro.Mobile.Core.Services.Authentication;

namespace RestaurantePro.Mobile.Core.Services.Authorization;

/// <summary>
/// Implementación del servicio de autorización basado en roles
/// Mapea roles del backend a permisos específicos de la app móvil
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthorizationService> _logger;
    
    // Mapeo estático de roles a permisos según la documentación del proyecto
    private static readonly Dictionary<string, List<AppPermission>> RolePermissions = new()
    {
        // MESEROS: CORE operativo - Gestión de comandas y mesas
        ["Mesero"] = new List<AppPermission>
        {
            // Comandas (funcionalidad principal)
            AppPermission.CrearComandas,
            AppPermission.VerComandas,
            AppPermission.ModificarComandas,
            AppPermission.CerrarComandas,
            AppPermission.EntregarComandas,
            AppPermission.FinalizarComandas,
            
            // Mesas (funcionalidad principal)
            AppPermission.VerEstadoMesas,
            AppPermission.CambiarEstadoMesas,
            AppPermission.AsignarMesas,
            
            // Productos (consulta para tomar órdenes)
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            
            // Clientes (básico para comandas)
            AppPermission.ConsultarClientes,
            AppPermission.UsarTarjetasFidelizacion,
            
            // Reservaciones (confirmar llegadas)
            AppPermission.ConsultarReservaciones,
            AppPermission.ConfirmarReservaciones,
            
            // Personal
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        },
        
        // COCINEROS: Estados de preparación y cocina
        ["Cocinero"] = new List<AppPermission>
        {
            // Preparaciones (funcionalidad principal)
            AppPermission.VerPreparacionesPendientes,
            AppPermission.ActualizarEstadoPreparaciones,
            AppPermission.CompletarPreparaciones,
            
            // Productos (consulta para preparación)
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            
            // Inventario (disponibilidad ingredientes)
            AppPermission.ConsultarDisponibilidadIngredientes,
            AppPermission.VerAlertasInventario,
            
            // Personal
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        },
        
        // CAJEROS: Facturación y pagos
        ["Cajero"] = new List<AppPermission>
        {
            // Facturación (funcionalidad principal)
            AppPermission.GenerarFacturas,
            AppPermission.ProcesarPagos,
            AppPermission.AplicarPromociones,
            
            // Comandas (solo lectura para facturar)
            AppPermission.VerComandas,
            AppPermission.CerrarComandas,
            
            // Productos (consulta para facturación)
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            
            // Clientes (gestión para facturación)
            AppPermission.ConsultarClientes,
            AppPermission.UsarTarjetasFidelizacion,
            
            // Personal
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        },
        
        // GERENTES/SUPERVISORES: Acceso amplio + supervisión
        ["Gerente"] = new List<AppPermission>
        {
            // Acceso a todas las funcionalidades operativas
            AppPermission.CrearComandas,
            AppPermission.VerComandas,
            AppPermission.ModificarComandas,
            AppPermission.CerrarComandas,
            AppPermission.VerEstadoMesas,
            AppPermission.CambiarEstadoMesas,
            AppPermission.AsignarMesas,
            AppPermission.VerPreparacionesPendientes,
            AppPermission.ActualizarEstadoPreparaciones,
            AppPermission.CompletarPreparaciones,
            AppPermission.GenerarFacturas,
            AppPermission.ProcesarPagos,
            AppPermission.AplicarPromociones,
            
            // Funcionalidades de consulta
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            AppPermission.ConsultarClientes,
            AppPermission.UsarTarjetasFidelizacion,
            AppPermission.ConsultarDisponibilidadIngredientes,
            AppPermission.VerAlertasInventario,
            AppPermission.ConsultarReservaciones,
            AppPermission.ConfirmarReservaciones,
            
            // Funcionalidades de supervisión
            AppPermission.VerReportesBasicos,
            AppPermission.GestionarPersonalTurno,
            AppPermission.SupervisarOperaciones,
            
            // Personal
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        },
        
        // ADMINISTRADORES: Acceso completo
        ["Administrador"] = Enum.GetValues<AppPermission>().ToList(),
        
        // ENCARGADO INVENTARIO: Inventario + operaciones básicas
        ["EncargadoInventario"] = new List<AppPermission>
        {
            // Inventario (funcionalidad principal)
            AppPermission.ConsultarDisponibilidadIngredientes,
            AppPermission.VerAlertasInventario,
            
            // Operaciones básicas de apoyo
            AppPermission.VerComandas,
            AppPermission.VerPreparacionesPendientes,
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            
            // Personal
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        },
        
        // EMPLEADO GENÉRICO: Funcionalidades básicas
        ["Empleado"] = new List<AppPermission>
        {
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos,
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        }
    };
    
    // Mapeo de funcionalidades a permisos requeridos
    private static readonly Dictionary<AppFeature, List<AppPermission>> FeaturePermissions = new()
    {
        [AppFeature.GestionComandas] = new List<AppPermission>
        {
            AppPermission.CrearComandas,
            AppPermission.VerComandas,
            AppPermission.ModificarComandas,
            AppPermission.CerrarComandas
        },
        
        [AppFeature.GestionMesas] = new List<AppPermission>
        {
            AppPermission.VerEstadoMesas,
            AppPermission.CambiarEstadoMesas,
            AppPermission.AsignarMesas
        },
        
        [AppFeature.Cocina] = new List<AppPermission>
        {
            AppPermission.VerPreparacionesPendientes,
            AppPermission.ActualizarEstadoPreparaciones,
            AppPermission.CompletarPreparaciones
        },
        
        [AppFeature.Caja] = new List<AppPermission>
        {
            AppPermission.GenerarFacturas,
            AppPermission.ProcesarPagos,
            AppPermission.AplicarPromociones
        },
        
        [AppFeature.ConsultaProductos] = new List<AppPermission>
        {
            AppPermission.ConsultarProductos,
            AppPermission.VerDetalleProductos
        },
        
        [AppFeature.AtencionCliente] = new List<AppPermission>
        {
            AppPermission.ConsultarClientes,
            AppPermission.UsarTarjetasFidelizacion
        },
        
        [AppFeature.ConsultaInventario] = new List<AppPermission>
        {
            AppPermission.ConsultarDisponibilidadIngredientes,
            AppPermission.VerAlertasInventario
        },
        
        [AppFeature.Reservaciones] = new List<AppPermission>
        {
            AppPermission.ConsultarReservaciones,
            AppPermission.ConfirmarReservaciones
        },
        
        [AppFeature.Supervision] = new List<AppPermission>
        {
            AppPermission.VerReportesBasicos,
            AppPermission.GestionarPersonalTurno,
            AppPermission.SupervisarOperaciones
        },
        
        [AppFeature.ConfiguracionPersonal] = new List<AppPermission>
        {
            AppPermission.ConfiguracionPersonal,
            AppPermission.VerPerfil
        }
    };

    public AuthorizationService(IAuthService authService, ILogger<AuthorizationService> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(AppPermission permission)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔐🎯 [HasPermission] Verificando permiso: {permission}");
            
            var userPermissions = await GetUserPermissionsAsync();
            var hasPermission = userPermissions.Contains(permission);
            
            System.Diagnostics.Debug.WriteLine($"🔐🎯 [HasPermission] Usuario tiene {userPermissions.Count} permisos");
            System.Diagnostics.Debug.WriteLine($"🔐🎯 [HasPermission] ¿Tiene '{permission}'? {hasPermission}");
            
            if (!hasPermission)
            {
                System.Diagnostics.Debug.WriteLine($"🔐❌ [HasPermission] Permiso '{permission}' NO encontrado en: [{string.Join(", ", userPermissions)}]");
            }
            
            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando permiso {Permission}", permission);
            System.Diagnostics.Debug.WriteLine($"🔐💥 [HasPermission] ERROR: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasAnyPermissionAsync(params AppPermission[] permissions)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync();
            return permissions.Any(p => userPermissions.Contains(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando permisos múltiples: {Permissions}", string.Join(", ", permissions));
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasAllPermissionsAsync(params AppPermission[] permissions)
    {
        try
        {
            var userPermissions = await GetUserPermissionsAsync();
            return permissions.All(p => userPermissions.Contains(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando todos los permisos: {Permissions}", string.Join(", ", permissions));
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasRoleAsync(string role)
    {
        try
        {
            var userRoles = await GetUserRolesAsync();
            return userRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando rol {Role}", role);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<List<AppPermission>> GetUserPermissionsAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            
            // DEBUG DETALLADO
            System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] GetCurrentUserAsync() result:");
            System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] - User: {user?.Email ?? "NULL"}");
            System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] - User.Roles: {(user?.Roles == null ? "NULL" : $"[{string.Join(", ", user.Roles)}]")}");
            System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] - Roles count: {user?.Roles?.Count ?? 0}");
            
            if (user == null || user.Roles == null || !user.Roles.Any())
            {
                _logger.LogWarning("Usuario no autenticado o sin roles asignados");
                System.Diagnostics.Debug.WriteLine("🔐❌ [AuthService] Usuario SIN ROLES - Retornando lista vacía");
                return new List<AppPermission>();
            }

            var allPermissions = new HashSet<AppPermission>();
            
            System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] Roles disponibles en RolePermissions: [{string.Join(", ", RolePermissions.Keys)}]");
            
            foreach (var role in user.Roles)
            {
                System.Diagnostics.Debug.WriteLine($"🔐🔍 [AuthService] Procesando rol: '{role}'");
                
                if (RolePermissions.TryGetValue(role, out var rolePermissions))
                {
                    System.Diagnostics.Debug.WriteLine($"🔐✅ [AuthService] Rol '{role}' ENCONTRADO - {rolePermissions.Count} permisos");
                    
                    foreach (var permission in rolePermissions)
                    {
                        allPermissions.Add(permission);
                        System.Diagnostics.Debug.WriteLine($"🔐➕ [AuthService] Añadido permiso: {permission}");
                    }
                }
                else
                {
                    _logger.LogWarning("Rol no reconocido: {Role}", role);
                    System.Diagnostics.Debug.WriteLine($"🔐❌ [AuthService] Rol '{role}' NO ENCONTRADO en RolePermissions");
                }
            }

            System.Diagnostics.Debug.WriteLine($"🔐🎯 [AuthService] Total permisos finales: {allPermissions.Count}");
            System.Diagnostics.Debug.WriteLine($"🔐🎯 [AuthService] Permisos: [{string.Join(", ", allPermissions)}]");
            
            return allPermissions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo permisos del usuario");
            System.Diagnostics.Debug.WriteLine($"🔐💥 [AuthService] EXCEPCIÓN: {ex.Message}");
            return new List<AppPermission>();
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetUserRolesAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.Roles ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo roles del usuario");
            return new List<string>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> CanAccessFeatureAsync(AppFeature feature)
    {
        try
        {
            if (!FeaturePermissions.TryGetValue(feature, out var requiredPermissions))
            {
                _logger.LogWarning("Funcionalidad no reconocida: {Feature}", feature);
                return false;
            }

            // Para acceder a una funcionalidad, debe tener al menos uno de los permisos requeridos
            return await HasAnyPermissionAsync(requiredPermissions.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando acceso a funcionalidad {Feature}", feature);
            return false;
        }
    }
}
