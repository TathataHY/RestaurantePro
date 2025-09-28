using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.Identity.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
        public bool Activo { get; set; } = true;
        public string FotoPerfil { get; set; }
        // ❌ DEPRECATED: Refresh token individual (reemplazado por RefreshTokens)
        // public string RefreshToken { get; set; }
        // public DateTime? RefreshTokenExpiryTime { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
} 