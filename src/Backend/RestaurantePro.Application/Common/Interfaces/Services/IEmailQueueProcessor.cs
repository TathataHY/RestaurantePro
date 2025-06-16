using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Common.Interfaces.Services;

/// <summary>
/// Define el contrato para un procesador de emails en segundo plano
/// </summary>
public interface IEmailQueueProcessor
{
    /// <summary>
    /// Procesa los emails pendientes en cola
    /// </summary>
    /// <param name="batchSize">Tamaño del lote a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de emails procesados</returns>
    Task<int> ProcessPendingEmailsAsync(int batchSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Encola un email para envío posterior
    /// </summary>
    /// <param name="destinatario">Dirección de correo del destinatario</param>
    /// <param name="asunto">Asunto del correo</param>
    /// <param name="contenido">Contenido HTML del correo</param>
    /// <param name="prioridad">Prioridad del correo (alta, normal, baja)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Identificador del email encolado</returns>
    Task<string> EnqueueEmailAsync(
        string destinatario, 
        string asunto, 
        string contenido, 
        string prioridad = "normal",
        CancellationToken cancellationToken = default);
} 