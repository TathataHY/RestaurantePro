namespace RestaurantePro.Domain.Comercial.Services
{
    /// <summary>
    /// Interfaz extendida para el servicio de fidelización con caché
    /// </summary>
    public interface IServicioFidelizacionCached : IServicioFidelizacion
    {
        /// <summary>
        /// Invalida toda la caché del servicio de fidelización
        /// </summary>
        void InvalidarCache();
    }
} 