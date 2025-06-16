using System;
using System.Threading.Tasks;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;

/// <summary>
/// Define las funcionalidades para un planificador de trabajos en segundo plano
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Agenda un trabajo para su ejecución inmediata
    /// </summary>
    /// <typeparam name="TJob">Tipo de trabajo a ejecutar</typeparam>
    /// <returns>Identificador del trabajo agendado</returns>
    Task<string> ScheduleJobAsync<TJob>() where TJob : IBackgroundJob;
    
    /// <summary>
    /// Agenda un trabajo para su ejecución retrasada
    /// </summary>
    /// <typeparam name="TJob">Tipo de trabajo a ejecutar</typeparam>
    /// <param name="delay">Tiempo a esperar antes de la ejecución</param>
    /// <returns>Identificador del trabajo agendado</returns>
    Task<string> ScheduleJobAsync<TJob>(TimeSpan delay) where TJob : IBackgroundJob;
    
    /// <summary>
    /// Agenda un trabajo recurrente
    /// </summary>
    /// <typeparam name="TJob">Tipo de trabajo a ejecutar</typeparam>
    /// <param name="cronExpression">Expresión cron para definir la recurrencia</param>
    /// <returns>Identificador del trabajo agendado</returns>
    Task<string> ScheduleRecurringJobAsync<TJob>(string cronExpression) where TJob : IBackgroundJob;
    
    /// <summary>
    /// Cancela un trabajo agendado
    /// </summary>
    /// <param name="jobId">Identificador del trabajo</param>
    /// <returns>True si el trabajo fue cancelado, false en caso contrario</returns>
    Task<bool> CancelJobAsync(string jobId);
} 