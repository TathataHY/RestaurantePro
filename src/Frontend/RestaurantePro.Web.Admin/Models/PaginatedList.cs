namespace RestaurantePro.Web.Admin.Models;

/// <summary>
/// Lista paginada de elementos
/// </summary>
/// <typeparam name="T">Tipo de elementos en la lista</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// Lista de elementos de la página actual
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// Número total de elementos en todas las páginas
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Número de página actual (base 1)
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Tamaño de página
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Número total de páginas
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    /// <summary>
    /// Indica si hay una página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
    
    /// <summary>
    /// Indica si hay una página siguiente
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
    
    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public PaginatedList()
    {
    }
    
    /// <summary>
    /// Constructor con parámetros
    /// </summary>
    /// <param name="items">Lista de elementos</param>
    /// <param name="totalCount">Total de elementos</param>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}