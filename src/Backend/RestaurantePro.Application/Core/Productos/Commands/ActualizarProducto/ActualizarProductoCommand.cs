namespace RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;

public class ActualizarProductoCommand : IRequest<Result<ProductoDto>>
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public Guid CategoriaId { get; set; }
    public bool Activo { get; set; } = true;
    public string? ImagenUrl { get; set; }
} 