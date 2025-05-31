namespace RestaurantePro.Application.Common.DTOs;

/// <summary>
/// Resultado paginado genérico con metadatos completos de paginación, filtrado y estadísticas
/// Incluye funcionalidades empresariales avanzadas para reportes y análisis
/// </summary>
/// <typeparam name="T">Tipo de elementos en la colección paginada</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Constructor vacío para deserialización
    /// </summary>
    public PagedResult()
    {
        Items = new List<T>();
        Metadata = new PaginationMetadata();
        FilterMetadata = new FilterMetadata();
        SortMetadata = new SortMetadata();
        Statistics = new ResultStatistics();
    }

    /// <summary>
    /// Constructor principal con todos los parámetros
    /// </summary>
    public PagedResult(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Dictionary<string, object>? filters = null,
        string? sortField = null,
        string? sortDirection = null)
    {
        Items = items.ToList();
        
        Metadata = new PaginationMetadata
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            HasPrevious = pageNumber > 1,
            HasNext = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        FilterMetadata = new FilterMetadata
        {
            SearchTerm = searchTerm,
            ActiveFilters = filters ?? new Dictionary<string, object>(),
            FilterCount = (filters?.Count ?? 0) + (string.IsNullOrWhiteSpace(searchTerm) ? 0 : 1)
        };

        SortMetadata = new SortMetadata
        {
            SortField = sortField,
            SortDirection = sortDirection ?? "asc"
        };

        Statistics = new ResultStatistics
        {
            ItemsOnCurrentPage = Items.Count,
            StartIndex = (pageNumber - 1) * pageSize + 1,
            EndIndex = Math.Min(pageNumber * pageSize, totalCount),
            ShowingFrom = totalCount > 0 ? (pageNumber - 1) * pageSize + 1 : 0,
            ShowingTo = Math.Min(pageNumber * pageSize, totalCount),
            ResultsText = GenerateResultsText()
        };

        // Calcular métricas avanzadas
        CalculateAdvancedMetrics();
    }

    /// <summary>
    /// Colección de elementos de la página actual
    /// </summary>
    public List<T> Items { get; set; }

    /// <summary>
    /// Metadatos de paginación básica
    /// </summary>
    public PaginationMetadata Metadata { get; set; }

    /// <summary>
    /// Metadatos de filtrado aplicado
    /// </summary>
    public FilterMetadata FilterMetadata { get; set; }

    /// <summary>
    /// Metadatos de ordenamiento aplicado
    /// </summary>
    public SortMetadata SortMetadata { get; set; }

    /// <summary>
    /// Estadísticas del resultado
    /// </summary>
    public ResultStatistics Statistics { get; set; }

    /// <summary>
    /// Información de rendimiento de la consulta
    /// </summary>
    public PerformanceInfo? Performance { get; set; }

    /// <summary>
    /// Enlaces de navegación para APIs REST
    /// </summary>
    public NavigationLinks? Links { get; set; }

    /// <summary>
    /// Configuración de exportación disponible
    /// </summary>
    public ExportOptions? ExportOptions { get; set; }

    // Factory Methods para diferentes tipos de resultados

    /// <summary>
    /// Crea un resultado paginado vacío
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PagedResult<T>(
            new List<T>(),
            0,
            pageNumber,
            pageSize
        );
    }

    /// <summary>
    /// Crea un resultado paginado simple sin filtros
    /// </summary>
    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Crea un resultado paginado con búsqueda
    /// </summary>
    public static PagedResult<T> CreateWithSearch(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string searchTerm)
    {
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize, searchTerm);
    }

    /// <summary>
    /// Crea un resultado paginado completo con filtros y ordenamiento
    /// </summary>
    public static PagedResult<T> CreateAdvanced(
        IEnumerable<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Dictionary<string, object>? filters = null,
        string? sortField = null,
        string? sortDirection = null,
        TimeSpan? executionTime = null,
        string? baseUrl = null)
    {
        var result = new PagedResult<T>(items, totalCount, pageNumber, pageSize, searchTerm, filters, sortField, sortDirection);
        
        if (executionTime.HasValue)
        {
            result.Performance = new PerformanceInfo
            {
                ExecutionTimeMs = executionTime.Value.TotalMilliseconds,
                QueryExecutedAt = DateTime.UtcNow,
                IsSlowQuery = executionTime.Value.TotalMilliseconds > 1000
            };
        }

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            result.Links = NavigationLinks.Generate(baseUrl, pageNumber, pageSize, result.Metadata.TotalPages);
        }

        result.ExportOptions = ExportOptions.GetAvailableFormats();

        return result;
    }

    /// <summary>
    /// Transforma el resultado paginado a otro tipo
    /// </summary>
    public PagedResult<TResult> Transform<TResult>(Func<T, TResult> selector)
    {
        var transformedItems = Items.Select(selector);
        
        return new PagedResult<TResult>(
            transformedItems,
            Metadata.TotalCount,
            Metadata.CurrentPage,
            Metadata.PageSize,
            FilterMetadata.SearchTerm,
            FilterMetadata.ActiveFilters,
            SortMetadata.SortField,
            SortMetadata.SortDirection)
        {
            Performance = Performance,
            Links = Links,
            ExportOptions = ExportOptions
        };
    }

    /// <summary>
    /// Calcula métricas avanzadas del resultado
    /// </summary>
    private void CalculateAdvancedMetrics()
    {
        Statistics.LoadFactor = Metadata.TotalCount > 0 ? (double)Statistics.ItemsOnCurrentPage / Metadata.PageSize : 0;
        Statistics.CompletionPercentage = Metadata.TotalPages > 0 ? (double)Metadata.CurrentPage / Metadata.TotalPages * 100 : 0;
        Statistics.RemainingItems = Math.Max(0, Metadata.TotalCount - Statistics.EndIndex);
        Statistics.RemainingPages = Math.Max(0, Metadata.TotalPages - Metadata.CurrentPage);
    }

    /// <summary>
    /// Genera texto descriptivo del resultado
    /// </summary>
    private string GenerateResultsText()
    {
        if (Metadata.TotalCount == 0)
            return "No se encontraron resultados";

        var hasFilters = FilterMetadata.FilterCount > 0;
        var filterText = hasFilters ? " (filtrados)" : "";
        
        if (Metadata.TotalCount == 1)
            return $"1 resultado{filterText}";

        if (Metadata.TotalPages == 1)
            return $"{Metadata.TotalCount} resultados{filterText}";

        return $"Mostrando {Statistics.ShowingFrom}-{Statistics.ShowingTo} de {Metadata.TotalCount} resultados{filterText}";
    }

    /// <summary>
    /// Verifica si hay resultados
    /// </summary>
    public bool HasResults => Items.Any();

    /// <summary>
    /// Verifica si es la primera página
    /// </summary>
    public bool IsFirstPage => Metadata.CurrentPage == 1;

    /// <summary>
    /// Verifica si es la última página
    /// </summary>
    public bool IsLastPage => Metadata.CurrentPage >= Metadata.TotalPages;

    /// <summary>
    /// Obtiene el número de página anterior (si existe)
    /// </summary>
    public int? PreviousPage => Metadata.HasPrevious ? Metadata.CurrentPage - 1 : null;

    /// <summary>
    /// Obtiene el número de página siguiente (si existe)
    /// </summary>
    public int? NextPage => Metadata.HasNext ? Metadata.CurrentPage + 1 : null;

    /// <summary>
    /// Obtiene rango de páginas para paginador
    /// </summary>
    public IEnumerable<int> GetPageRange(int maxPages = 10)
    {
        var start = Math.Max(1, Metadata.CurrentPage - maxPages / 2);
        var end = Math.Min(Metadata.TotalPages, start + maxPages - 1);
        
        // Ajustar el inicio si estamos cerca del final
        if (end - start + 1 < maxPages)
        {
            start = Math.Max(1, end - maxPages + 1);
        }

        return Enumerable.Range(start, end - start + 1);
    }
}

/// <summary>
/// Metadatos básicos de paginación
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; } = 0;
    public int TotalPages { get; set; } = 0;
    public bool HasPrevious { get; set; } = false;
    public bool HasNext { get; set; } = false;
}

/// <summary>
/// Metadatos de filtrado aplicado
/// </summary>
public class FilterMetadata
{
    public string? SearchTerm { get; set; }
    public Dictionary<string, object> ActiveFilters { get; set; } = new();
    public int FilterCount { get; set; } = 0;
    public DateTime? FilterAppliedAt { get; set; }
    public string? FilterDescription { get; set; }
}

/// <summary>
/// Metadatos de ordenamiento aplicado
/// </summary>
public class SortMetadata
{
    public string? SortField { get; set; }
    public string SortDirection { get; set; } = "asc";
    public List<string> AvailableSortFields { get; set; } = new();
    public bool IsDefaultSort { get; set; } = true;
}

/// <summary>
/// Estadísticas del resultado
/// </summary>
public class ResultStatistics
{
    public int ItemsOnCurrentPage { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public int ShowingFrom { get; set; }
    public int ShowingTo { get; set; }
    public string ResultsText { get; set; } = string.Empty;
    public double LoadFactor { get; set; }
    public double CompletionPercentage { get; set; }
    public int RemainingItems { get; set; }
    public int RemainingPages { get; set; }
}

/// <summary>
/// Información de rendimiento de la consulta
/// </summary>
public class PerformanceInfo
{
    public double ExecutionTimeMs { get; set; }
    public DateTime QueryExecutedAt { get; set; }
    public bool IsSlowQuery { get; set; }
    public string? CacheStatus { get; set; }
    public int? RecordsScanned { get; set; }
    public string? IndexesUsed { get; set; }
}

/// <summary>
/// Enlaces de navegación para APIs REST
/// </summary>
public class NavigationLinks
{
    public string? First { get; set; }
    public string? Previous { get; set; }
    public string? Current { get; set; }
    public string? Next { get; set; }
    public string? Last { get; set; }

    public static NavigationLinks Generate(string baseUrl, int currentPage, int pageSize, int totalPages)
    {
        var links = new NavigationLinks();
        
        if (!baseUrl.Contains('?'))
            baseUrl += "?";
        else if (!baseUrl.EndsWith('&'))
            baseUrl += "&";

        links.First = $"{baseUrl}page=1&pageSize={pageSize}";
        links.Last = $"{baseUrl}page={totalPages}&pageSize={pageSize}";
        links.Current = $"{baseUrl}page={currentPage}&pageSize={pageSize}";

        if (currentPage > 1)
        {
            links.Previous = $"{baseUrl}page={currentPage - 1}&pageSize={pageSize}";
        }

        if (currentPage < totalPages)
        {
            links.Next = $"{baseUrl}page={currentPage + 1}&pageSize={pageSize}";
        }

        return links;
    }
}

/// <summary>
/// Opciones de exportación disponibles
/// </summary>
public class ExportOptions
{
    public List<ExportFormat> AvailableFormats { get; set; } = new();
    public int MaxExportRecords { get; set; } = 10000;
    public bool RequiresAuthentication { get; set; } = true;
    public TimeSpan EstimatedExportTime { get; set; }

    public static ExportOptions GetAvailableFormats()
    {
        return new ExportOptions
        {
            AvailableFormats = new List<ExportFormat>
            {
                new() { Format = "CSV", DisplayName = "CSV (Excel)", MimeType = "text/csv", MaxRecords = 50000 },
                new() { Format = "XLSX", DisplayName = "Excel", MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MaxRecords = 100000 },
                new() { Format = "PDF", DisplayName = "PDF", MimeType = "application/pdf", MaxRecords = 5000 },
                new() { Format = "JSON", DisplayName = "JSON", MimeType = "application/json", MaxRecords = 25000 }
            },
            MaxExportRecords = 100000,
            RequiresAuthentication = true,
            EstimatedExportTime = TimeSpan.FromSeconds(30)
        };
    }
}

/// <summary>
/// Formato de exportación disponible
/// </summary>
public class ExportFormat
{
    public string Format { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int MaxRecords { get; set; }
    public List<string> SupportedFields { get; set; } = new();
}

/// <summary>
/// Extensiones útiles para PagedResult
/// </summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// Convierte una lista en un PagedResult simulando paginación en memoria
    /// </summary>
    public static PagedResult<T> ToPagedResult<T>(
        this IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        return PagedResult<T>.Create(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Aplica filtros de búsqueda a un PagedResult
    /// </summary>
    public static PagedResult<T> WithSearch<T>(
        this PagedResult<T> result,
        string searchTerm,
        Func<T, bool> searchPredicate)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return result;

        var filteredItems = result.Items.Where(searchPredicate).ToList();
        result.Items = filteredItems;
        result.FilterMetadata.SearchTerm = searchTerm;
        result.FilterMetadata.FilterCount++;
        
        return result;
    }

    /// <summary>
    /// Agrega información de rendimiento al resultado
    /// </summary>
    public static PagedResult<T> WithPerformance<T>(
        this PagedResult<T> result,
        TimeSpan executionTime,
        string? cacheStatus = null)
    {
        result.Performance = new PerformanceInfo
        {
            ExecutionTimeMs = executionTime.TotalMilliseconds,
            QueryExecutedAt = DateTime.UtcNow,
            IsSlowQuery = executionTime.TotalMilliseconds > 1000,
            CacheStatus = cacheStatus
        };
        
        return result;
    }

    /// <summary>
    /// Agrega enlaces de navegación al resultado
    /// </summary>
    public static PagedResult<T> WithLinks<T>(
        this PagedResult<T> result,
        string baseUrl)
    {
        result.Links = NavigationLinks.Generate(
            baseUrl,
            result.Metadata.CurrentPage,
            result.Metadata.PageSize,
            result.Metadata.TotalPages);
        
        return result;
    }
} 