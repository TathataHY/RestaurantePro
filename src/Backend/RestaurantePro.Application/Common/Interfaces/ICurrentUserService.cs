namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para obtener información sobre el usuario actual
/// Específica de la capa Application para acceso al contexto del usuario
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el ID del usuario actual
    /// </summary>
    /// <returns>ID del usuario o null si no está autenticado</returns>
    Guid? UserId { get; }
    
    /// <summary>
    /// Obtiene el email del usuario actual
    /// </summary>
    /// <returns>Email del usuario o null si no está autenticado</returns>
    string? UserEmail { get; }
    
    /// <summary>
    /// Obtiene el nombre de usuario actual
    /// </summary>
    /// <returns>Nombre del usuario o null si no está autenticado</returns>
    string? UserName { get; }
    
    /// <summary>
    /// Obtiene los roles del usuario actual
    /// </summary>
    /// <returns>Lista de roles del usuario</returns>
    IEnumerable<string> UserRoles { get; }
    
    /// <summary>
    /// Verifica si el usuario actual está autenticado
    /// </summary>
    /// <returns>True si está autenticado, False en caso contrario</returns>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Verifica si el usuario actual tiene un rol específico
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <returns>True si tiene el rol, False en caso contrario</returns>
    bool IsInRole(string role);
} 