namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de permisos y roles de usuarios
/// Maneja la lógica de autorización y jerarquías organizacionales
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    /// Verifica si un usuario tiene un permiso específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="permiso">Nombre del permiso a verificar</param>
    /// <returns>True si el usuario tiene el permiso</returns>
    Task<bool> UsuarioTienePermisoAsync(Guid usuarioId, string permiso);
    
    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="rol">Nombre del rol a verificar</param>
    /// <returns>True si el usuario tiene el rol</returns>
    Task<bool> UsuarioTieneRolAsync(Guid usuarioId, string rol);
    
    /// <summary>
    /// Verifica si un usuario tiene nivel de acceso suficiente
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="nivelRequerido">Nivel mínimo requerido (1-10)</param>
    /// <returns>True si el usuario tiene el nivel suficiente</returns>
    Task<bool> UsuarioTieneNivelAccesoAsync(Guid usuarioId, int nivelRequerido);
    
    /// <summary>
    /// Obtiene todos los permisos de un usuario
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Lista de permisos del usuario</returns>
    Task<List<string>> ObtenerPermisosUsuarioAsync(Guid usuarioId);
    
    /// <summary>
    /// Obtiene los usuarios subordinados de un supervisor
    /// </summary>
    /// <param name="supervisorId">ID del supervisor</param>
    /// <returns>Lista de IDs de usuarios subordinados</returns>
    Task<List<Guid>> ObtenerSubordinadosAsync(Guid supervisorId);
    
    /// <summary>
    /// Verifica si un usuario puede supervisar a otro
    /// </summary>
    /// <param name="supervisorId">ID del potencial supervisor</param>
    /// <param name="subordinadoId">ID del potencial subordinado</param>
    /// <returns>True si puede supervisar</returns>
    Task<bool> PuedeSupervisarAsync(Guid supervisorId, Guid subordinadoId);
    
    /// <summary>
    /// Obtiene los usuarios del mismo departamento
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>Lista de IDs de usuarios del mismo departamento</returns>
    Task<List<Guid>> ObtenerUsuariosMismoDepartamentoAsync(Guid usuarioId);
    
    /// <summary>
    /// Verifica si un usuario es administrador del sistema
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <returns>True si es administrador</returns>
    Task<bool> EsAdministradorAsync(Guid usuarioId);
} 