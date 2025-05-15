using Microsoft.AspNetCore.Identity;
using System;

namespace RestaurantePro.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rol { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimaModificacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
    }
} 