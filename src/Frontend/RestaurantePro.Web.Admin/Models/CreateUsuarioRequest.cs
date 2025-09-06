using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

public class CreateUsuarioRequest
{
    [Required]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(40, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty;

    public string? Telefono { get; set; }
}


