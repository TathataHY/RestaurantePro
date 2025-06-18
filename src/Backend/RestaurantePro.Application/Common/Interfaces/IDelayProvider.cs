namespace RestaurantePro.Application.Common.Interfaces;

/// <summary>
/// Provee una abstracción para Task.Delay para facilitar las pruebas.
/// </summary>
public interface IDelayProvider
{
    /// <summary>
    /// Espera un período de tiempo.
    /// </summary>
    /// <param name="delay">El período de tiempo de espera.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>A Task that completes after a time delay.</returns>
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
} 