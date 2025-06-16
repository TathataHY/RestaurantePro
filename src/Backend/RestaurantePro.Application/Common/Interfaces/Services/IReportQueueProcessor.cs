using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Application.Common.Interfaces.Services;

/// <summary>
/// Define el contrato para un procesador de reportes en cola
/// </summary>
public interface IReportQueueProcessor
{
    /// <summary>
    /// Procesa los reportes pendientes en cola
    /// </summary>
    /// <param name="batchSize">Tamaño del lote a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de reportes procesados</returns>
    Task<int> ProcessPendingReportsAsync(int batchSize, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Encola una solicitud de generación de reporte
    /// </summary>
    /// <param name="tipoReporte">Tipo de reporte a generar</param>
    /// <param name="parametros">Parámetros para la generación del reporte</param>
    /// <param name="usuarioId">ID del usuario que solicita el reporte</param>
    /// <param name="formatoSalida">Formato de salida deseado</param>
    /// <param name="notificarComplecion">Indica si se debe notificar al usuario cuando esté completo</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Identificador de la solicitud de reporte</returns>
    Task<Guid> EnqueueReportRequestAsync(
        string tipoReporte,
        object parametros,
        string usuarioId,
        string formatoSalida = "PDF",
        bool notificarComplecion = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene el estado de un reporte solicitado
    /// </summary>
    /// <param name="reporteId">ID del reporte</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado actual del reporte</returns>
    Task<string> GetReportStatusAsync(Guid reporteId, CancellationToken cancellationToken = default);
} 