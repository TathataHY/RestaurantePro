namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// DTO para productos del menú (contenido en español, tipo en inglés).
/// </summary>
public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int Popularidad { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
