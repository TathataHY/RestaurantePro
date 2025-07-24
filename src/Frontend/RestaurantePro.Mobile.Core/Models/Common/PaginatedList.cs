namespace RestaurantePro.Mobile.Core.Models.Common;

/// <summary>
/// Clase simple para manejar respuestas paginadas desde la API
/// </summary>
public class PaginatedList<T>
{
    /// <summary>
    /// Lista de elementos en la página actual
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Número total de elementos
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Número de página actual
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
} 