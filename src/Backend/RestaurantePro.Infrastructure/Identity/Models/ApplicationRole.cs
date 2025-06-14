using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.Identity.Models
{
    /// <summary>
    /// Modelo de rol para Identity con propiedades extendidas
    /// </summary>
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base()
        {
            RolePermissions = new List<ApplicationRolePermission>();
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
            RolePermissions = new List<ApplicationRolePermission>();
        }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fecha de creación del rol
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si es un rol de sistema (no modificable)
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Permisos asociados a este rol
        /// </summary>
        public virtual ICollection<ApplicationRolePermission> RolePermissions { get; set; }
    }
} 