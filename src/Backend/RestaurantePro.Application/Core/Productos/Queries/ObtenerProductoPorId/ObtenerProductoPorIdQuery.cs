namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;

/// <summary>
/// Query para obtener un producto por su ID
/// Vertical Slice: ObtenerProductoPorId
/// </summary>
public class ObtenerProductoPorIdQuery : IRequest<Result<ProductoDto>>
{
    public Guid ProductoId { get; set; }

    public ObtenerProductoPorIdQuery(Guid productoId)
    {
        ProductoId = productoId;
    }
} 