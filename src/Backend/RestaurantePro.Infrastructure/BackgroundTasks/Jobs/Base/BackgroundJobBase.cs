using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Base;

/// <summary>
/// Clase base para los trabajos en segundo plano que proporciona funcionalidad común
/// </summary>
public abstract class BackgroundJobBase : IRecurringJob
{
    protected readonly ILogger<BackgroundJobBase> _logger;
    private readonly string _jobName;
    
    /// <inheritdoc/>
    public string JobId => $"{JobName}_{Guid.NewGuid():N}";
    
    /// <inheritdoc/>
    public abstract string JobName { get; }
    
    /// <inheritdoc/>
    public abstract string Description { get; }

    protected BackgroundJobBase(ILogger<BackgroundJobBase> logger)
    {
        _logger = logger;
        _jobName = GetType().Name;
    }

    /// <inheritdoc/>
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando trabajo: {JobName}", _jobName);
        try
        {
            await ExecuteInternalAsync(cancellationToken);
            _logger.LogInformation("Trabajo {JobName} completado exitosamente.", _jobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando el trabajo: {JobName}", _jobName);
        }
    }

    public Task Execute(CancellationToken none)
    {
        return ExecuteAsync(none);
    }

    /// <summary>
    /// Implementación interna de la ejecución del trabajo
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);
} 