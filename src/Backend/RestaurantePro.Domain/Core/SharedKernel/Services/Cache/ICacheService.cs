using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services.Cache
{
    /// <summary>
    /// Interfaz para el servicio de caché
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Obtiene un valor de la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <returns>Valor o default si no existe</returns>
        T Get<T>(string key);
        
        /// <summary>
        /// Obtiene un valor de la caché de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Valor o default si no existe</returns>
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica si una clave existe en la caché
        /// </summary>
        /// <param name="key">Clave a verificar</param>
        /// <returns>True si existe, false si no</returns>
        bool Exists(string key);
        
        /// <summary>
        /// Verifica si una clave existe en la caché de forma asíncrona
        /// </summary>
        /// <param name="key">Clave a verificar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si existe, false si no</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Establece un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expirationMinutes">Minutos hasta la expiración</param>
        void Set<T>(string key, T value, int expirationMinutes = 60);
        
        /// <summary>
        /// Establece un valor en la caché de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expirationMinutes">Minutos hasta la expiración</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Establece un valor en la caché de forma asíncrona con tiempo de expiración
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expiration">Tiempo de expiración</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Elimina un valor de la caché
        /// </summary>
        /// <param name="key">Clave del valor a eliminar</param>
        void Remove(string key);
        
        /// <summary>
        /// Elimina un valor de la caché de forma asíncrona
        /// </summary>
        /// <param name="key">Clave del valor a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalida todas las claves que coincidan con un patrón
        /// </summary>
        /// <param name="pattern">Patrón de claves a invalidar</param>
        void InvalidatePattern(string pattern);
        
        /// <summary>
        /// Invalida todas las claves que coincidan con un patrón de forma asíncrona
        /// </summary>
        /// <param name="pattern">Patrón de claves a invalidar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo crea si no existe
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="factory">Función para crear el valor si no existe</param>
        /// <param name="expirationMinutes">Minutos hasta la expiración</param>
        /// <returns>El valor obtenido o creado</returns>
        T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo crea si no existe (alias para GetOrCreate)
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="loadFunc">Función para cargar el valor si no existe</param>
        /// <param name="timeToLiveMinutes">Minutos hasta la expiración</param>
        /// <returns>El valor obtenido o creado</returns>
        T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo crea si no existe de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="loadFunc">Función asíncrona para cargar el valor si no existe</param>
        /// <param name="timeToLiveMinutes">Minutos hasta la expiración</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El valor obtenido o creado</returns>
        Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default);
    }
} 