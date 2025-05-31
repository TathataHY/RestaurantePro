using Microsoft.Extensions.Caching.Memory;

namespace RestaurantePro.Application.Common.Behaviors;

/// <summary>
/// Behavior para implementar caché en consultas de solo lectura
/// Este es un ejemplo básico que se puede expandir con Redis o MemoryCache
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly IMemoryCache _memoryCache;

    public CachingBehavior(
        ILogger<CachingBehavior<TRequest, TResponse>> logger,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Solo aplicar caché a consultas (queries), no a comandos
        if (!IsQuery(request))
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var cacheKey = GenerateCacheKey(request);

        // Intentar obtener del caché
        if (_memoryCache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogDebug("📚 Cache HIT para {RequestName}", requestName);
            return cachedResponse!;
        }

        // Ejecutar la operación original
        _logger.LogDebug("📝 Cache MISS para {RequestName}", requestName);
        var response = await next();

        // Guardar en caché con expiración
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = GetCacheExpiration(request),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(cacheKey, response, cacheOptions);
        _logger.LogDebug("💾 Guardado en caché: {RequestName}", requestName);

        return response;
    }

    /// <summary>
    /// Determina si la request es una consulta (Query) o un comando
    /// </summary>
    private static bool IsQuery(TRequest request)
    {
        // Asumimos que todas las consultas tienen "Query" en su nombre
        return typeof(TRequest).Name.Contains("Query", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Genera una clave única para el caché basada en el tipo y contenido de la request
    /// </summary>
    private static string GenerateCacheKey(TRequest request)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);
        var hash = requestJson.GetHashCode();
        return $"{requestName}_{hash}";
    }

    /// <summary>
    /// Determina el tiempo de expiración del caché según el tipo de consulta
    /// </summary>
    private static TimeSpan GetCacheExpiration(TRequest request)
    {
        var requestName = typeof(TRequest).Name.ToLowerInvariant();

        // Configuraciones específicas por tipo de consulta
        return requestName switch
        {
            var name when name.Contains("obtenerproducto") => TimeSpan.FromMinutes(15), // Productos cambian poco
            var name when name.Contains("obtenerusuario") => TimeSpan.FromMinutes(10),   // Usuarios cambian moderadamente
            var name when name.Contains("obtenercomanda") => TimeSpan.FromMinutes(2),    // Comandas cambian rápido
            var name when name.Contains("paginados") => TimeSpan.FromMinutes(5),         // Listas paginadas
            _ => TimeSpan.FromMinutes(10) // Tiempo por defecto
        };
    }
} 