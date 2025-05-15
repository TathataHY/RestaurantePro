using System;

namespace RestaurantePro.Domain.Entities
{
    /// <summary>
    /// Representa un usuario del sistema con sus credenciales y rol
    /// </summary>
    public class Usuario : BaseEntity
    {
        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string Apellido { get; set; }

        /// <summary>
        /// Correo electrónico del usuario (único)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Hash de la contraseña del usuario
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Salt usado para la generación del hash
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Rol del usuario (Mesero, Cocinero, Gerente, Administrador)
        /// </summary>
        public string Rol { get; set; }

        /// <summary>
        /// Último acceso del usuario al sistema
        /// </summary>
        public DateTime? UltimoAcceso { get; set; }
    }
} 