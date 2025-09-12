using RestaurantePro.Domain.Core.Base.Services;

namespace RestaurantePro.Web.Admin.IntegrationTests.Services;

/// <summary>
/// Implementación de prueba para IDateTimeService
/// </summary>
public class TestDateTimeService : IDateTimeService
{
    private static readonly DateTime _fixedDateTime = DateTime.Now;

    /// <summary>
    /// Obtiene la fecha y hora actual
    /// </summary>
    public DateTime Now => _fixedDateTime;

    /// <summary>
    /// Obtiene la fecha actual (sin hora)
    /// </summary>
    public DateTime Today => _fixedDateTime.Date;

    /// <summary>
    /// Obtiene la fecha y hora actual en UTC
    /// </summary>
    public DateTime UtcNow => _fixedDateTime.ToUniversalTime();
}
