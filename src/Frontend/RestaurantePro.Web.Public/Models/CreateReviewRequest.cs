namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// Request para crear una reseña pública (contenido en español, tipo en inglés).
/// </summary>
public class CreateReviewRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Valoracion { get; set; }
}


