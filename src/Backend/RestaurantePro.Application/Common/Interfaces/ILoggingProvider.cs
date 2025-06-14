using Microsoft.Extensions.Logging;

namespace RestaurantePro.Application.Common.Interfaces
{
    /// <summary>
    /// Interfaz para proveedores de logging
    /// </summary>
    public interface ILoggingProvider
    {
        /// <summary>
        /// Configura y devuelve un logger
        /// </summary>
        /// <returns>El logger configurado</returns>
        ILogger ConfigureLogger();
    }
} 