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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Verificar cancelación antes de continuar
        cancellationToken.ThrowIfCancellationRequested();
        
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

        // Verificar cancelación antes de ejecutar la operación
        cancellationToken.ThrowIfCancellationRequested();

        // Ejecutar la operación original
        _logger.LogDebug("📝 Cache MISS para {RequestName}", requestName);
        var response = await next();

        // Calcular expiración
        var expiration = GetCacheExpiration(request);

        // Si TTL es 0 o negativo, no cachear esta respuesta
        if (expiration <= TimeSpan.Zero)
        {
            return response;
        }

        // Guardar en caché con expiración
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        // Usar CreateEntry en lugar del extension method Set para mejor compatibilidad con testing
        using var entry = _memoryCache.CreateEntry(cacheKey);
        entry.Value = response;
        entry.AbsoluteExpirationRelativeToNow = cacheOptions.AbsoluteExpirationRelativeToNow;
        entry.SlidingExpiration = cacheOptions.SlidingExpiration;
        entry.Priority = cacheOptions.Priority;
        entry.Size = 1; // Especificar tamaño para que funcione con SizeLimit
        
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
        // Regla específica: todas las consultas de Mesas SIN caché (estado cambia muy seguido)
        if (requestName.Contains("mesa"))
        {
            return TimeSpan.Zero;
        }

        // Regla específica: todas las consultas de Comandas SIN caché (estado cambia segundo a segundo)
        if (requestName.Contains("comanda"))
        {
            return TimeSpan.Zero;
        }

        // Regla específica: todas las consultas de Preparaciones Diarias SIN caché (cambian frecuentemente)
        if (requestName.Contains("preparacion") || requestName.Contains("preparaciones"))
        {
            return TimeSpan.Zero;
        }

        // Regla específica: listados paginados de productos SIN caché; otros paginados se mantienen breves
        if (requestName.Contains("paginados"))
        {
            if (requestName.Contains("producto"))
            {
                return TimeSpan.Zero;
            }
            // SIN caché para usuarios paginados (datos cambian frecuentemente)
            if (requestName.Contains("usuario"))
            {
                return TimeSpan.Zero;
            }
            return TimeSpan.FromSeconds(15);
        }

        return requestName switch
        {
            var name when name.Contains("obtenerproductoporid") => TimeSpan.FromSeconds(10), // Detalle de producto: TTL corto
            var name when name.Contains("obtenerproducto") => TimeSpan.FromMinutes(15), // Otros productos: TTL estándar
            var name when name.Contains("obtenerusuario") => TimeSpan.Zero,   // SIN caché para usuarios (datos cambian frecuentemente)
            var name when name.Contains("obtenercomanda") => TimeSpan.FromSeconds(15),   // Comandas cambian muy rápido (cocina)
            var name when name.Contains("obtenermesa") => TimeSpan.FromSeconds(30),      // Mesas cambian muy rápido
            var name when name.Contains("obtenermesaporid") => TimeSpan.FromSeconds(15), // Mesa individual cambia muy rápido
            _ => TimeSpan.FromMinutes(10) // Tiempo por defecto
        };
    }
} 