using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;

/// <summary>
/// Define la estructura básica para todos los trabajos en segundo plano
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Identificador único del trabajo
    /// </summary>
    string JobId { get; }
    
    /// <summary>
    /// Nombre descriptivo del trabajo
    /// </summary>
    string JobName { get; }
    
    /// <summary>
    /// Descripción de la función del trabajo
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Ejecuta el trabajo de manera asíncrona
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task que representa la operación asíncrona</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
} 