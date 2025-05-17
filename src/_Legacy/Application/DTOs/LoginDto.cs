using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Application.DTOs
{
    /// <summary>
    /// DTO para datos de inicio de sesión
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Email del usuario
        /// </summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public string Email { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }
    }
} 