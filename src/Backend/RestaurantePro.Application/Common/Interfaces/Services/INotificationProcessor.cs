using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Common.Interfaces.Services;

/// <summary>
/// Define el contrato para un procesador de notificaciones en segundo plano
/// </summary>
public interface INotificationProcessor
{
    /// <summary>
    /// Procesa las notificaciones pendientes en cola
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de notificaciones procesadas</returns>
    Task<int> ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Procesa un lote específico de notificaciones pendientes
    /// </summary>
    /// <param name="batchSize">Tamaño del lote a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de notificaciones procesadas</returns>
    Task<int> ProcessPendingNotificationsAsync(int batchSize, CancellationToken cancellationToken = default);
} 