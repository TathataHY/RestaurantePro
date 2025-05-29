namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;

public class ObtenerProductosPorCategoriaQuery : IRequest<Result<List<ProductoDto>>>
{
    /// <summary>
    /// ID de la categoría
    /// </summary>
    public Guid CategoriaId { get; set; }

    /// <summary>
    /// Solo productos activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Ordenar por popularidad
    /// </summary>
    public bool OrdenarPorPopularidad { get; set; } = false;

    public ObtenerProductosPorCategoriaQuery(Guid categoriaId, bool soloActivos = true, bool ordenarPorPopularidad = false)
    {
        CategoriaId = categoriaId;
        SoloActivos = soloActivos;
        OrdenarPorPopularidad = ordenarPorPopularidad;
    }

    public ObtenerProductosPorCategoriaQuery() { }
} 