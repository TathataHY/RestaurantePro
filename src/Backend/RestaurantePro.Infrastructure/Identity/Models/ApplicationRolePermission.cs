using System;

namespace RestaurantePro.Infrastructure.Identity.Models
{
    /// <summary>
    /// Modelo para la relación entre roles y permisos
    /// </summary>
    public class ApplicationRolePermission
    {
        /// <summary>
        /// Identificador único
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Rol asociado
        /// </summary>
        public virtual ApplicationRole Role { get; set; }

        /// <summary>
        /// Nombre del permiso
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si el permiso está activo
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 