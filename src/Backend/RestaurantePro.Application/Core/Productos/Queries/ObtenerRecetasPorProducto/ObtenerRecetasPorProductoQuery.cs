using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetasPorProducto;

/// <summary>
/// Query para obtener las recetas asociadas a un producto específico
/// </summary>
public class ObtenerRecetasPorProductoQuery : IRequest<Result<List<RecetaDto>>>
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Incluir solo recetas activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Incluir información de ingredientes
    /// </summary>
    public bool IncluirIngredientes { get; set; } = true;

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerRecetasPorProductoQuery() { }

    /// <summary>
    /// Constructor con ID del producto
    /// </summary>
    public ObtenerRecetasPorProductoQuery(Guid productoId)
    {
        ProductoId = productoId;
    }
} 