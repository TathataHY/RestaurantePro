using System;

namespace RestaurantePro.Domain.Core.SharedKernel.Services
{
    /// <summary>
    /// Interfaz para estrategias de cálculo de TTL (tiempo de vida) dinámico para caché
    /// </summary>
    public interface IDynamicTtlStrategy
    {
        /// <summary>
        /// Calcula el TTL óptimo para una clave de caché específica basado en su patrón de uso
        /// </summary>
        /// <param name="key">Clave de caché</param>
        /// <param name="defaultTtlMinutes">TTL predeterminado en minutos</param>
        /// <returns>TTL calculado en minutos</returns>
        int CalculateTtl(string key, int defaultTtlMinutes);
        
        /// <summary>
        /// Registra un acceso a una clave para análisis de patrones
        /// </summary>
        /// <param name="key">Clave accedida</param>
        /// <param name="isHit">Si fue un acierto o no</param>
        /// <param name="operationType">Tipo de operación</param>
        void RegisterAccess(string key, bool isHit, string operationType);
        
        /// <summary>
        /// Registra una invalidación para análisis de patrones
        /// </summary>
        /// <param name="pattern">Patrón invalidado</param>
        /// <param name="affectedKeys">Número de claves afectadas</param>
        void RegisterInvalidation(string pattern, int affectedKeys);
    }
} 