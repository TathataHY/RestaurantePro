namespace RestaurantePro.Web.Admin.Models;

public class ProductoDto
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Popularidad { get; set; }
    public bool Activo { get; set; }
}


