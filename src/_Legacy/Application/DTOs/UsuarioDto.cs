using System;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para transferencia de datos de usuario
    /// </summary>
    public class UsuarioDto
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        public string Apellido { get; set; }

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Rol del usuario
        /// </summary>
        public string Rol { get; set; }

        /// <summary>
        /// Indica si el usuario está activo
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Fecha de creación del usuario
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Fecha de última actualización del usuario
        /// </summary>
        public DateTime FechaActualizacion { get; set; }
    }
} 