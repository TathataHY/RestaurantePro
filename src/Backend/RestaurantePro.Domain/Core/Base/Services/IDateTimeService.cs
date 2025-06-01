namespace RestaurantePro.Domain.Core.Base.Services;

/// <summary>
/// Interfaz para servicios de fecha y hora en el dominio
/// Abstracción que permite al dominio obtener fecha/hora sin depender de implementaciones
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
    /// Obtiene solo la fecha actual
    /// </summary>
    DateTime Today { get; }
} 