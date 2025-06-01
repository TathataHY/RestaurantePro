namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Interfaz para el servicio de fecha y hora
/// Permite abstraer la obtención del tiempo actual para testing
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Obtiene la fecha y hora actual
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Obtiene la fecha y hora actual en UTC
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Obtiene solo la fecha actual sin hora
    /// </summary>
    DateTime Today { get; }

    /// <summary>
    /// Convierte una fecha a UTC
    /// </summary>
    /// <param name="dateTime">Fecha a convertir</param>
    /// <returns>Fecha en UTC</returns>
    DateTime ToUtc(DateTime dateTime);

    /// <summary>
    /// Convierte una fecha UTC a hora local
    /// </summary>
    /// <param name="utcDateTime">Fecha UTC</param>
    /// <returns>Fecha en hora local</returns>
    DateTime ToLocal(DateTime utcDateTime);
} 