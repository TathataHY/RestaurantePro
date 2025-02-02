using Microsoft.AspNetCore.Identity;
using RestaurantePro.Core.Entities;
using RestaurantePro.Core.Enums;

namespace RestaurantePro.Core.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool IsActive { get; set; } = true;
        public RolUsuario Rol { get; set; }
        public virtual ICollection<Comanda> Comandas { get; set; }
    }
} 