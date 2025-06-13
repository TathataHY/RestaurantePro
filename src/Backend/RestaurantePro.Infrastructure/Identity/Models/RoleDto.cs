namespace RestaurantePro.Infrastructure.Identity.Models
{
    /// <summary>
    /// DTO para representar un rol con sus permisos
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Lista de permisos asignados al rol
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();
    }
} 