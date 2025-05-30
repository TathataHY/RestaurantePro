namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Servicio para obtener información del usuario actual
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID del usuario actual
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Nombre del usuario actual
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Email del usuario actual
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Indica si el usuario está autenticado
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Roles del usuario actual
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico
    /// </summary>
    /// <param name="role">Rol a verificar</param>
    /// <returns>True si tiene el rol</returns>
    bool IsInRole(string role);
} 