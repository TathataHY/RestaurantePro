namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// DTO de reseña pública (contenido en español, tipo en inglés).
/// </summary>
public class ReviewDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Valoracion { get; set; }
    public DateTime Fecha { get; set; }
}


