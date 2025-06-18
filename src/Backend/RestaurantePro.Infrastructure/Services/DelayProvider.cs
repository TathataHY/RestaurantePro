using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Infrastructure.Services;

/// <summary>
/// Implementación de producción de IDelayProvider que usa Task.Delay.
/// </summary>
public class DelayProvider : IDelayProvider
{
    /// <inheritdoc />
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }
} 