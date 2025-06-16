using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Application.Common.Interfaces.Services;

namespace RestaurantePro.Infrastructure.BackgroundTasks.Workers;

/// <summary>
/// Worker en segundo plano que procesa notificaciones pendientes
/// </summary>
public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly NotificationWorkerOptions _options;

    public NotificationWorker(
        ILogger<NotificationWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<NotificationWorkerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Worker iniciado a las {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Worker de notificaciones ejecutándose a las {time}", DateTimeOffset.Now);
                
                // Procesamiento de las notificaciones pendientes
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationProcessor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                    await notificationProcessor.ProcessPendingNotificationsAsync(stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error en el procesamiento de notificaciones: {Message}", ex.Message);
                
                // En caso de error, esperamos un poco antes de reintentar
                await Task.Delay(_options.ErrorRetryInterval, stoppingToken);
            }

            // Esperamos el tiempo configurado antes de la próxima ejecución
            await Task.Delay(_options.ProcessingInterval, stoppingToken);
        }
    }
}

/// <summary>
/// Opciones de configuración para el worker de notificaciones
/// </summary>
public class NotificationWorkerOptions
{
    /// <summary>
    /// Intervalo entre procesamiento de notificaciones en milisegundos
    /// </summary>
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(30);
    
    /// <summary>
    /// Intervalo de reintento en caso de error en milisegundos
    /// </summary>
    public TimeSpan ErrorRetryInterval { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// Número máximo de notificaciones a procesar en cada iteración
    /// </summary>
    public int BatchSize { get; set; } = 100;
} 