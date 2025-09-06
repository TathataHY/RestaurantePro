namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// DTO de mensaje de contacto (contenido en español, tipo en inglés).
/// </summary>
public class ContactMessageDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}


