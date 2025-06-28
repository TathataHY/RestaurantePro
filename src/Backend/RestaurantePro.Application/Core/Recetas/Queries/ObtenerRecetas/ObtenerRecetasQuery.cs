using RestaurantePro.Application.Core.Recetas.DTOs;
using RestaurantePro.Application.Common.DTOs;

namespace RestaurantePro.Application.Core.Recetas.Queries.ObtenerRecetas;

/// <summary>
/// Query para obtener recetas paginadas con filtros opcionales
/// </summary>
public class ObtenerRecetasQuery : IRequest<Result<PaginatedList<RecetaDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (máximo 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Filtrar solo recetas activas
    /// </summary>
    public bool? SoloActivas { get; set; }

    /// <summary>
    /// Filtrar por producto específico
    /// </summary>
    public Guid? ProductoId { get; set; }

    /// <summary>
    /// Filtro de texto libre (nombre de receta, instrucciones)
    /// </summary>
    public string? FiltroTexto { get; set; }

    /// <summary>
    /// Campo de ordenamiento
    /// </summary>
    public string OrdenarPor { get; set; } = "FechaCreacion";

    /// <summary>
    /// Dirección del ordenamiento (Asc/Desc)
    /// </summary>
    public string DireccionOrden { get; set; } = "Desc";

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
    public ObtenerRecetasQuery(int pageNumber = 1, int pageSize = 20, bool? soloActivas = null, Guid? productoId = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SoloActivas = soloActivas;
        ProductoId = productoId;
    }

    /// <summary>
    /// Factory method para búsqueda básica
    /// </summary>
    public static ObtenerRecetasQuery Crear(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize);

    /// <summary>
    /// Factory method para búsqueda con filtro de texto
    /// </summary>
    public static ObtenerRecetasQuery CrearConFiltro(string filtroTexto, int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize) { FiltroTexto = filtroTexto };

    /// <summary>
    /// Factory method para recetas activas
    /// </summary>
    public static ObtenerRecetasQuery CrearSoloActivas(int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize) { SoloActivas = true };

    /// <summary>
    /// Factory method para recetas de un producto específico
    /// </summary>
    public static ObtenerRecetasQuery CrearPorProducto(Guid productoId, int pageNumber = 1, int pageSize = 20)
        => new(pageNumber, pageSize) { ProductoId = productoId };
} 
