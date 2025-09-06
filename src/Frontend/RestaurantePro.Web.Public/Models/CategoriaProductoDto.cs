namespace RestaurantePro.Web.Public.Models;

/// <summary>
/// DTO para categorías de productos (contenido en español, tipo en inglés).
/// </summary>
public class CategoriaProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196F3";
    public string Icono { get; set; } = "🍽️";
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public int CantidadProductos { get; set; }
    public int ProductosDisponibles { get; set; }
    public DateTime FechaCreacion { get; set; }
}


