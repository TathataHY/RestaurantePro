namespace RestaurantePro.Application.Common.DTOs;

/// <summary>
/// DTO base para solicitudes con filtros estándar
/// Proporciona funcionalidad común de filtrado, ordenamiento y paginación
/// </summary>
public class FilterRequest
{
    /// <summary>
    /// Número de página (inicia en 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Tamaño de página (cantidad de elementos por página)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Texto de búsqueda general
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Campo por el cual ordenar los resultados
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Dirección del ordenamiento (asc/desc)
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Filtrar solo elementos activos
    /// </summary>
    public bool SoloActivos { get; set; } = true;

    /// <summary>
    /// Fecha desde para filtrar por rango de fechas
    /// </summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>
    /// Fecha hasta para filtrar por rango de fechas
    /// </summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>
    /// Validaciones básicas del filtro
    /// </summary>
    public bool IsValid => PageNumber > 0 && PageSize > 0 && PageSize <= 100;

    /// <summary>
    /// Calcula el número de elementos a omitir para la paginación
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Enumeración para la dirección del ordenamiento
/// </summary>
public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// FilterRequest específico para productos con filtros adicionales
/// </summary>
public class ProductoFilterRequest : FilterRequest
{
    /// <summary>
    /// Filtrar por categoría específica
    /// </summary>
    public Guid? CategoriaId { get; set; }

    /// <summary>
    /// Precio mínimo
    /// </summary>
    public decimal? PrecioMinimo { get; set; }

    /// <summary>
    /// Precio máximo
    /// </summary>
    public decimal? PrecioMaximo { get; set; }

    /// <summary>
    /// Nivel mínimo de popularidad (0-10)
    /// </summary>
    public int? PopularidadMinima { get; set; }
} 