namespace RestaurantePro.Application.Core.Usuarios.DTOs;

/// <summary>
/// DTO para representar un rol con sus permisos
/// </summary>
public class RoleDto
{
    /// <summary>
    /// ID del rol
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del rol
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista de permisos asignados al rol
    /// </summary>
    public List<string> Permissions { get; set; } = new List<string>();
} 