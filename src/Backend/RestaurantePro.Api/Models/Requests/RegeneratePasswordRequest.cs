using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Api.Models.Requests
{
    /// <summary>
    /// Modelo de request para regenerar contraseña de un usuario
    /// </summary>
    public class RegeneratePasswordRequest
    {
        /// <summary>
        /// Email del usuario al que se le regenerará la contraseña
        /// </summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nueva contraseña para el usuario
        /// </summary>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
