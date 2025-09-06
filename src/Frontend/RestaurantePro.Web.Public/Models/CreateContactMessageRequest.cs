using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// Request para enviar un mensaje de contacto.
/// </summary>
public class CreateContactMessageRequest
{
    [Required]
    [StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [StringLength(120)]
    public string Asunto { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Mensaje { get; set; } = string.Empty;
}


