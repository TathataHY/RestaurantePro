namespace RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;

public class ObtenerProductosPaginadosQuery : IRequest<Result<PaginatedList<ProductoDto>>>
{
    /// <summary>
    /// Número de página (base 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filtro de búsqueda por nombre o descripción
    /// </summary>
    public string? Filtro { get; set; }

    /// <summary>
    /// Filtrar por categoría específica
    /// </summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>
    /// Solo productos activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Precio mínimo para filtrar
    /// </summary>
    public decimal? PrecioMinimo { get; set; }

    /// <summary>
    /// Precio máximo para filtrar
    /// </summary>
    public decimal? PrecioMaximo { get; set; }

    /// <summary>
    /// Fecha de creación desde
    /// </summary>
    public DateTime? FechaCreacionDesde { get; set; }

    /// <summary>
    /// Fecha de creación hasta
    /// </summary>
    public DateTime? FechaCreacionHasta { get; set; }

    /// <summary>
    /// Nivel de popularidad mínimo (0-10)
    /// </summary>
    public int? PopularidadMinima { get; set; }

    /// <summary>
    /// Nivel de popularidad máximo (0-10)
    /// </summary>
    public int? PopularidadMaxima { get; set; }

    /// <summary>
    /// Campo para ordenar los resultados
    /// </summary>
    public string OrderBy { get; set; } = "Nombre";

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public string OrderDirection { get; set; } = "asc";
} 