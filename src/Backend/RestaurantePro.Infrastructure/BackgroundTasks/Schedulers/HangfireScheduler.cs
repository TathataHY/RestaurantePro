using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;
using Hangfire;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Schedulers;

/// <summary>
/// Implementación del planificador de trabajos usando Hangfire
/// </summary>
public class HangfireScheduler : IJobScheduler
{
    private readonly ILogger<HangfireScheduler> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HangfireScheduler(
        ILogger<HangfireScheduler> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<string> ScheduleJobAsync<TJob>() where TJob : IBackgroundJob
    {
        var jobId = BackgroundJob.Enqueue<TJob>(job => job.ExecuteAsync(default));
        _logger.LogInformation("Trabajo {JobType} agendado con ID: {JobId}", typeof(TJob).Name, jobId);
        return Task.FromResult(jobId);
    }

    /// <inheritdoc/>
    public Task<string> ScheduleJobAsync<TJob>(TimeSpan delay) where TJob : IBackgroundJob
    {
        var jobId = BackgroundJob.Schedule<TJob>(job => job.ExecuteAsync(default), delay);
        _logger.LogInformation("Trabajo {JobType} agendado con delay de {Delay} con ID: {JobId}", 
            typeof(TJob).Name, delay, jobId);
        return Task.FromResult(jobId);
    }

    /// <inheritdoc/>
    public Task<string> ScheduleRecurringJobAsync<TJob>(string cronExpression) where TJob : IBackgroundJob
    {
        var jobId = $"{typeof(TJob).Name}_{Guid.NewGuid():N}";
        RecurringJob.AddOrUpdate<TJob>(jobId, job => job.ExecuteAsync(default), cronExpression);
        _logger.LogInformation("Trabajo recurrente {JobType} agendado con expresión cron {CronExpression} con ID: {JobId}", 
            typeof(TJob).Name, cronExpression, jobId);
        return Task.FromResult(jobId);
    }

    /// <inheritdoc/>
    public Task<bool> CancelJobAsync(string jobId)
    {
        var result = BackgroundJob.Delete(jobId);
        if (result)
        {
            _logger.LogInformation("Trabajo con ID: {JobId} cancelado exitosamente", jobId);
        }
        else
        {
            _logger.LogWarning("No se pudo cancelar el trabajo con ID: {JobId}", jobId);
        }
        return Task.FromResult(result);
    }
} 