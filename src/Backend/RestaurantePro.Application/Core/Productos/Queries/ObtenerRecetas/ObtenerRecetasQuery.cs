using RestaurantePro.Application.Core.Productos.DTOs;

namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerRecetas;

/// <summary>
/// Query para obtener todas las recetas disponibles con filtros opcionales
/// </summary>
public class ObtenerRecetasQuery : IRequest<Result<List<RecetaDto>>>
{
    /// <summary>
    /// Filtrar solo recetas activas
    /// </summary>
    public bool? SoloActivas { get; set; }

    /// <summary>
    /// Filtrar por producto específico
    /// </summary>
    public Guid? ProductoId { get; set; }

    /// <summary>
    /// Incluir información de ingredientes
    /// </summary>
    public bool IncluirIngredientes { get; set; } = true;

    /// <summary>
    /// Incluir información del producto
    /// </summary>
    public bool IncluirProducto { get; set; } = true;

    /// <summary>
    /// Constructor sin parámetros
    /// </summary>
    public ObtenerRecetasQuery() { }

    /// <summary>
    /// Constructor con filtros básicos
    /// </summary>
    public ObtenerRecetasQuery(bool? soloActivas = null, Guid? productoId = null)
    {
        SoloActivas = soloActivas;
        ProductoId = productoId;
    }
} 