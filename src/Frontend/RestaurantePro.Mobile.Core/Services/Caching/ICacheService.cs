namespace RestaurantePro.Mobile.Core.Services.Caching;

/// <summary>
/// Contrato mínimo de caché que puede usar el proyecto Core sin depender de implementaciones de Mobile
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtiene un elemento del caché o lo calcula y guarda si no existe
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}


