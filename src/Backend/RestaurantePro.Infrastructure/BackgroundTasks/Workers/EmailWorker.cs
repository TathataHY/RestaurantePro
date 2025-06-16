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
/// Worker en segundo plano que procesa los correos electrónicos pendientes de envío
/// </summary>
public class EmailWorker : BackgroundService
{
    private readonly ILogger<EmailWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmailWorkerOptions _options;

    public EmailWorker(
        ILogger<EmailWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<EmailWorkerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Worker iniciado a las {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Worker de emails ejecutándose a las {time}", DateTimeOffset.Now);
                
                // Procesamiento de los emails pendientes
                using (var scope = _serviceProvider.CreateScope())
                {
                    var emailProcessor = scope.ServiceProvider.GetRequiredService<IEmailQueueProcessor>();
                    var processed = await emailProcessor.ProcessPendingEmailsAsync(_options.BatchSize, stoppingToken);
                    
                    if (processed > 0)
                    {
                        _logger.LogInformation("Se procesaron {Count} emails pendientes", processed);
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error en el procesamiento de emails: {Message}", ex.Message);
                
                // En caso de error, esperamos antes de reintentar
                await Task.Delay(_options.ErrorRetryInterval, stoppingToken);
            }

            // Esperamos el tiempo configurado antes de la próxima ejecución
            await Task.Delay(_options.ProcessingInterval, stoppingToken);
        }
    }

    /// <inheritdoc/>
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Servicio de procesamiento de emails iniciando");
        return base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Servicio de procesamiento de emails detenido");
        return base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Opciones de configuración para el worker de emails
/// </summary>
public class EmailWorkerOptions
{
    /// <summary>
    /// Intervalo entre procesamiento de emails en milisegundos
    /// </summary>
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(60);
    
    /// <summary>
    /// Intervalo de reintento en caso de error en milisegundos
    /// </summary>
    public TimeSpan ErrorRetryInterval { get; set; } = TimeSpan.FromSeconds(15);
    
    /// <summary>
    /// Número máximo de emails a procesar en cada iteración
    /// </summary>
    public int BatchSize { get; set; } = 50;
} 