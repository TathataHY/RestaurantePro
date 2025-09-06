using System.ComponentModel.DataAnnotations;

namespace RestaurantePro.Web.Admin.Models;

public class CreateProductoRequest
{
    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0, 10000000)]
    public decimal Precio { get; set; }

    [Required]
    public Guid CategoriaId { get; set; }

    public bool Activo { get; set; } = true;
}


