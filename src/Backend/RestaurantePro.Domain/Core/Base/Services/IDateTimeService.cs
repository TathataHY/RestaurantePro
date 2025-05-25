namespace RestaurantePro.Domain.Core.Base.Services
{
    /// <summary>
    /// Interface para el servicio de fecha y hora
    /// </summary>
    public interface IDateTimeService
    {
        /// <summary>
        /// Obtiene la fecha y hora actuales
        /// </summary>
        DateTime Now { get; }
        
        /// <summary>
        /// Obtiene la fecha actual (sin hora)
        /// </summary>
        DateTime Today { get; }
        
        /// <summary>
        /// Obtiene la fecha y hora actuales en UTC
        /// </summary>
        DateTime UtcNow { get; }
    }
} 