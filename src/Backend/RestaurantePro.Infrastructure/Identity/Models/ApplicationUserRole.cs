using Microsoft.AspNetCore.Identity;
using System;

namespace RestaurantePro.Infrastructure.Identity.Models
{
    /// <summary>
    /// Modelo para la relación entre usuarios y roles con propiedades extendidas
    /// </summary>
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        /// <summary>
        /// Usuario asociado
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        /// <summary>
        /// Rol asociado
        /// </summary>
        public virtual ApplicationRole Role { get; set; }

        /// <summary>
        /// Fecha de asignación del rol
        /// </summary>
        public DateTime AssignedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Usuario que asignó el rol
        /// </summary>
        public string AssignedBy { get; set; }

        /// <summary>
        /// Fecha de expiración del rol (opcional)
        /// </summary>
        public DateTime? ExpiresOn { get; set; }

        /// <summary>
        /// Indica si la asignación está activa
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
} 