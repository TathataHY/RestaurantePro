namespace RestaurantePro.Application.Common.Extensions;

/// <summary>
/// Extensiones útiles para IQueryable
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Aplica paginación a un IQueryable
    /// </summary>
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Aplica filtros dinámicos basados en FilterRequest
    /// </summary>
    public static IQueryable<T> ApplyFilter<T>(
        this IQueryable<T> source,
        FilterRequest filter) where T : class
    {
        var result = source;

        // Aplicar búsqueda por texto si se especifica
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            result = result.ApplyTextSearch(filter.SearchTerm);
        }

        // Aplicar filtro de fecha
        if (filter.FechaDesde.HasValue || filter.FechaHasta.HasValue)
        {
            result = result.ApplyDateFilter(filter.FechaDesde, filter.FechaHasta);
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var sortDirection = filter.SortDirection?.ToLower() == "desc" 
                ? SortDirection.Descending 
                : SortDirection.Ascending;
            result = result.ApplyOrdering(filter.SortBy, sortDirection);
        }

        return result;
    }

    /// <summary>
    /// Aplica búsqueda de texto en propiedades string de la entidad
    /// </summary>
    private static IQueryable<T> ApplyTextSearch<T>(this IQueryable<T> source, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return source;

        var stringProperties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .ToList();

        if (!stringProperties.Any())
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? searchExpression = null;

        foreach (var property in stringProperties)
        {
            var propertyExpression = Expression.Property(parameter, property);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var searchValue = Expression.Constant(searchTerm);
            var containsExpression = Expression.Call(propertyExpression, containsMethod, searchValue);

            searchExpression = searchExpression == null 
                ? containsExpression 
                : Expression.OrElse(searchExpression, containsExpression);
        }

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            source = source.Where(lambda);
        }

        return source;
    }

    /// <summary>
    /// Aplica filtro de fechas
    /// </summary>
    private static IQueryable<T> ApplyDateFilter<T>(
        this IQueryable<T> source, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta)
    {
        var dateProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name == "FechaCreacion" && p.PropertyType == typeof(DateTime));

        if (dateProperty == null)
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyExpression = Expression.Property(parameter, dateProperty);

        if (fechaDesde.HasValue)
        {
            var greaterThanOrEqual = Expression.GreaterThanOrEqual(
                propertyExpression, 
                Expression.Constant(fechaDesde.Value));
            var lambda = Expression.Lambda<Func<T, bool>>(greaterThanOrEqual, parameter);
            source = source.Where(lambda);
        }

        if (fechaHasta.HasValue)
        {
            var lessThanOrEqual = Expression.LessThanOrEqual(
                propertyExpression, 
                Expression.Constant(fechaHasta.Value));
            var lambda = Expression.Lambda<Func<T, bool>>(lessThanOrEqual, parameter);
            source = source.Where(lambda);
        }

        return source;
    }

    /// <summary>
    /// Aplica ordenamiento dinámico
    /// </summary>
    private static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> source, 
        string sortBy, 
        SortDirection sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return source;

        var property = typeof(T).GetProperty(sortBy);
        if (property == null)
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyExpression = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(propertyExpression, parameter);

        var methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
        var orderByMethod = typeof(Queryable).GetMethods()
            .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
            .Single()
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { source, lambda })!;
    }
} 