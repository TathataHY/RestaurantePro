namespace RestaurantePro.Application.Core.Productos.Commands.CrearProducto;

/// <summary>
/// Command para crear un nuevo producto en el catálogo
/// Vertical Slice: CrearProducto
/// </summary>
public class CrearProductoCommand : IRequest<Result<ProductoDto>>
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public bool Activo { get; set; } = true;
}

// NOTA: El DTO ProductoDto ahora se encuentra en:
// RestaurantePro.Application.Core.Productos.DTOs.ProductoDto 