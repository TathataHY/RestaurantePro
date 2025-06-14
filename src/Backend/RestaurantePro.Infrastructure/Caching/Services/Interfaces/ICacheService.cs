using System;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.Caching.Services.Interfaces
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
        /// <param name="key">Clave</param>
        /// <returns>Valor almacenado o default(T) si no existe</returns>
        T Get<T>(string key);

        /// <summary>
        /// Obtiene un valor de la caché de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <returns>Valor almacenado o default(T) si no existe</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Establece un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expiration">Tiempo de expiración (opcional)</param>
        void Set<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Establece un valor en la caché de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expiration">Tiempo de expiración (opcional)</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Elimina un valor de la caché
        /// </summary>
        /// <param name="key">Clave</param>
        void Remove(string key);

        /// <summary>
        /// Elimina un valor de la caché de forma asíncrona
        /// </summary>
        /// <param name="key">Clave</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Verifica si una clave existe en la caché
        /// </summary>
        /// <param name="key">Clave</param>
        /// <returns>True si existe, false en caso contrario</returns>
        bool Exists(string key);

        /// <summary>
        /// Verifica si una clave existe en la caché de forma asíncrona
        /// </summary>
        /// <param name="key">Clave</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Obtiene o crea un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="factory">Función para crear el valor si no existe</param>
        /// <param name="expiration">Tiempo de expiración (opcional)</param>
        /// <returns>Valor almacenado o creado</returns>
        T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Obtiene o crea un valor en la caché de forma asíncrona
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave</param>
        /// <param name="factory">Función para crear el valor si no existe</param>
        /// <param name="expiration">Tiempo de expiración (opcional)</param>
        /// <returns>Valor almacenado o creado</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

        /// <summary>
        /// Limpia toda la caché
        /// </summary>
        void Clear();

        /// <summary>
        /// Limpia toda la caché de forma asíncrona
        /// </summary>
        Task ClearAsync();
    }
} 