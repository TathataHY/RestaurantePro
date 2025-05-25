using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
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
        /// Verifica si una clave existe en la caché
        /// </summary>
        /// <param name="key">Clave a verificar</param>
        /// <returns>True si existe, false si no</returns>
        bool Exists(string key);
        
        /// <summary>
        /// Establece un valor en la caché
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="value">Valor a almacenar</param>
        /// <param name="expirationMinutes">Minutos hasta la expiración</param>
        void Set<T>(string key, T value, int expirationMinutes = 60);
        
        /// <summary>
        /// Elimina un valor de la caché
        /// </summary>
        /// <param name="key">Clave del valor a eliminar</param>
        void Remove(string key);
        
        /// <summary>
        /// Invalida todas las claves que coincidan con un patrón
        /// </summary>
        /// <param name="pattern">Patrón de claves a invalidar</param>
        void InvalidatePattern(string pattern);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo crea si no existe
        /// </summary>
        /// <typeparam name="T">Tipo del valor</typeparam>
        /// <param name="key">Clave del valor</param>
        /// <param name="factory">Función para crear el valor si no existe</param>
        /// <param name="expirationMinutes">Minutos hasta la expiración</param>
        /// <returns>Valor de la caché o recién creado</returns>
        T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo carga usando la función proporcionada si no existe
        /// </summary>
        /// <typeparam name="T">Tipo del valor a obtener</typeparam>
        /// <param name="key">Clave única para identificar el valor</param>
        /// <param name="loadFunc">Función para cargar el valor si no está en caché</param>
        /// <param name="timeToLiveMinutes">Tiempo de vida en minutos (0 para no expirar)</param>
        /// <returns>El valor desde la caché o el valor recién cargado</returns>
        T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10);
        
        /// <summary>
        /// Obtiene un valor de la caché o lo carga de forma asíncrona usando la función proporcionada si no existe
        /// </summary>
        /// <typeparam name="T">Tipo del valor a obtener</typeparam>
        /// <param name="key">Clave única para identificar el valor</param>
        /// <param name="loadFunc">Función asíncrona para cargar el valor si no está en caché</param>
        /// <param name="timeToLiveMinutes">Tiempo de vida en minutos (0 para no expirar)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>El valor desde la caché o el valor recién cargado</returns>
        Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default);
    }
} 