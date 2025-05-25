namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Interfaz para el servicio de caché utilizado para mejorar el rendimiento
    /// de operaciones costosas y frecuentes.
    /// </summary>
    public interface ICacheService
    {
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
        
        /// <summary>
        /// Elimina un valor específico de la caché
        /// </summary>
        /// <param name="key">Clave del valor a eliminar</param>
        /// <returns>True si se eliminó, False si no existía</returns>
        bool Remove(string key);
        
        /// <summary>
        /// Invalida todas las entradas de la caché que coincidan con el patrón de clave
        /// </summary>
        /// <param name="keyPattern">Patrón de clave (puede ser un prefijo o un regex dependiendo de la implementación)</param>
        /// <returns>Número de entradas eliminadas</returns>
        int InvalidatePattern(string keyPattern);
        
        /// <summary>
        /// Limpia toda la caché
        /// </summary>
        void Clear();
    }
} 