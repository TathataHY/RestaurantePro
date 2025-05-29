namespace RestaurantePro.Application.Common.DTOs;

/// <summary>
/// Representa una lista paginada genérica
/// </summary>
/// <typeparam name="T">Tipo de elementos en la lista</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Número de página actual (base 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Tamaño de página (elementos por página)
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de elementos en todas las páginas
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Indica si hay página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica si hay página siguiente
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public PaginatedList() { }

    /// <summary>
    /// Constructor que calcula automáticamente las propiedades de paginación
    /// </summary>
    /// <param name="items">Elementos de la página actual</param>
    /// <param name="totalCount">Total de elementos</param>
    /// <param name="pageNumber">Número de página actual</param>
    /// <param name="pageSize">Tamaño de página</param>
    public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    /// <summary>
    /// Crea una lista paginada vacía
    /// </summary>
    public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedList<T>(new List<T>(), 0, pageNumber, pageSize);
    }
} 