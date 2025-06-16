using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

/// <summary>
/// Clase base para los trabajos en segundo plano que proporciona funcionalidad común
/// </summary>
public abstract class BackgroundJobBase : IBackgroundJob
{
    protected readonly ILogger _logger;
    
    /// <inheritdoc/>
    public string JobId => $"{JobName}_{Guid.NewGuid():N}";
    
    /// <inheritdoc/>
    public abstract string JobName { get; }
    
    /// <inheritdoc/>
    public abstract string Description { get; }

    protected BackgroundJobBase(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando ejecución del trabajo {JobName} con ID {JobId}", JobName, JobId);
            
            await ExecuteInternalAsync(cancellationToken);
            
            _logger.LogInformation("Trabajo {JobName} con ID {JobId} completado exitosamente", JobName, JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la ejecución del trabajo {JobName} con ID {JobId}: {ErrorMessage}",
                JobName, JobId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Implementación interna de la ejecución del trabajo
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);
} 