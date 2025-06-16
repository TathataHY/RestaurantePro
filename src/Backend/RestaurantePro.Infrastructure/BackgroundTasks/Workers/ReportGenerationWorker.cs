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
/// Worker en segundo plano que se encarga de la generación programada de reportes
/// </summary>
public class ReportGenerationWorker : BackgroundService
{
    private readonly ILogger<ReportGenerationWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ReportGenerationWorkerOptions _options;

    public ReportGenerationWorker(
        ILogger<ReportGenerationWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<ReportGenerationWorkerOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Report Generation Worker iniciado a las {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.Now;
                _logger.LogDebug("Worker de reportes ejecutándose a las {time}", now);
                
                // Verificar si es hora de generar los reportes programados
                if (ShouldGenerateReports(now))
                {
                    _logger.LogInformation("Iniciando generación de reportes programados");
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
                        
                        // Generar reportes diarios
                        if (_options.GenerateDailyReports && IsTimeForDailyReports(now))
                        {
                            await GenerateDailyReportsAsync(reportService, stoppingToken);
                        }
                        
                        // Generar reportes semanales
                        if (_options.GenerateWeeklyReports && IsTimeForWeeklyReports(now))
                        {
                            await GenerateWeeklyReportsAsync(reportService, stoppingToken);
                        }
                        
                        // Generar reportes mensuales
                        if (_options.GenerateMonthlyReports && IsTimeForMonthlyReports(now))
                        {
                            await GenerateMonthlyReportsAsync(reportService, stoppingToken);
                        }
                    }
                }
                
                // Procesar reportes solicitados bajo demanda
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reportQueueProcessor = scope.ServiceProvider.GetRequiredService<IReportQueueProcessor>();
                    var processed = await reportQueueProcessor.ProcessPendingReportsAsync(_options.BatchSize, stoppingToken);
                    
                    if (processed > 0)
                    {
                        _logger.LogInformation("Se procesaron {Count} solicitudes de reportes pendientes", processed);
                    }
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error en la generación de reportes: {Message}", ex.Message);
                
                // En caso de error, esperamos antes de reintentar
                await Task.Delay(_options.ErrorRetryInterval, stoppingToken);
            }

            // Esperamos hasta el próximo intervalo de verificación
            await Task.Delay(_options.CheckInterval, stoppingToken);
        }
    }
    
    private bool ShouldGenerateReports(DateTimeOffset now)
    {
        // Verificar si estamos dentro del horario configurado para generación de reportes
        var hour = now.Hour;
        return hour >= _options.ReportGenerationStartHour && 
               hour <= _options.ReportGenerationEndHour;
    }
    
    private bool IsTimeForDailyReports(DateTimeOffset now)
    {
        // Reportes diarios a la hora configurada
        return now.Hour == _options.DailyReportHour;
    }
    
    private bool IsTimeForWeeklyReports(DateTimeOffset now)
    {
        // Reportes semanales en el día de semana configurado y a la hora configurada
        return (int)now.DayOfWeek == _options.WeeklyReportDay && 
               now.Hour == _options.WeeklyReportHour;
    }
    
    private bool IsTimeForMonthlyReports(DateTimeOffset now)
    {
        // Reportes mensuales en el día del mes configurado y a la hora configurada
        return now.Day == _options.MonthlyReportDay && 
               now.Hour == _options.MonthlyReportHour;
    }
    
    private async Task GenerateDailyReportsAsync(IReportService reportService, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generando reportes diarios");
        
        try
        {
            // Generar reportes diarios de ventas
            var ventasReport = await reportService.GenerarReporteDiarioVentasAsync(DateTime.Now.AddDays(-1), cancellationToken);
            _logger.LogInformation("Reporte diario de ventas generado: {ReportId}", ventasReport.Id);
            
            // Generar reportes diarios de inventario
            var inventarioReport = await reportService.GenerarReporteDiarioInventarioAsync(DateTime.Now.AddDays(-1), cancellationToken);
            _logger.LogInformation("Reporte diario de inventario generado: {ReportId}", inventarioReport.Id);
            
            // Otros reportes diarios...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reportes diarios: {Message}", ex.Message);
            throw;
        }
    }
    
    private async Task GenerateWeeklyReportsAsync(IReportService reportService, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generando reportes semanales");
        
        try
        {
            // Calcular fechas para la semana anterior
            var hoy = DateTime.Now.Date;
            var finSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioSemana = finSemana.AddDays(-6);
            
            // Generar reportes semanales de ventas
            var ventasReport = await reportService.GenerarReporteSemanalVentasAsync(inicioSemana, finSemana, cancellationToken);
            _logger.LogInformation("Reporte semanal de ventas generado: {ReportId}", ventasReport.Id);
            
            // Otros reportes semanales...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reportes semanales: {Message}", ex.Message);
            throw;
        }
    }
    
    private async Task GenerateMonthlyReportsAsync(IReportService reportService, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generando reportes mensuales");
        
        try
        {
            // Calcular fechas para el mes anterior
            var hoy = DateTime.Now.Date;
            var inicioMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-1);
            var finMesAnterior = new DateTime(hoy.Year, hoy.Month, 1).AddDays(-1);
            
            // Generar reportes mensuales de ventas
            var ventasReport = await reportService.GenerarReporteMensualVentasAsync(inicioMesAnterior, finMesAnterior, cancellationToken);
            _logger.LogInformation("Reporte mensual de ventas generado: {ReportId}", ventasReport.Id);
            
            // Generar reportes mensuales de operaciones
            var operacionesReport = await reportService.GenerarReporteMensualOperacionesAsync(inicioMesAnterior, finMesAnterior, cancellationToken);
            _logger.LogInformation("Reporte mensual de operaciones generado: {ReportId}", operacionesReport.Id);
            
            // Otros reportes mensuales...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reportes mensuales: {Message}", ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Opciones de configuración para el worker de generación de reportes
/// </summary>
public class ReportGenerationWorkerOptions
{
    /// <summary>
    /// Intervalo de comprobación para la generación de reportes
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(15);
    
    /// <summary>
    /// Intervalo de reintento en caso de error
    /// </summary>
    public TimeSpan ErrorRetryInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Número máximo de reportes a procesar por lote
    /// </summary>
    public int BatchSize { get; set; } = 10;
    
    /// <summary>
    /// Hora de inicio para la generación de reportes (0-23)
    /// </summary>
    public int ReportGenerationStartHour { get; set; } = 1; // 1:00 AM
    
    /// <summary>
    /// Hora de fin para la generación de reportes (0-23)
    /// </summary>
    public int ReportGenerationEndHour { get; set; } = 6; // 6:00 AM
    
    /// <summary>
    /// Indica si se deben generar reportes diarios
    /// </summary>
    public bool GenerateDailyReports { get; set; } = true;
    
    /// <summary>
    /// Hora para la generación de reportes diarios (0-23)
    /// </summary>
    public int DailyReportHour { get; set; } = 2; // 2:00 AM
    
    /// <summary>
    /// Indica si se deben generar reportes semanales
    /// </summary>
    public bool GenerateWeeklyReports { get; set; } = true;
    
    /// <summary>
    /// Día de la semana para la generación de reportes semanales (0=Domingo, 6=Sábado)
    /// </summary>
    public int WeeklyReportDay { get; set; } = 1; // Lunes
    
    /// <summary>
    /// Hora para la generación de reportes semanales (0-23)
    /// </summary>
    public int WeeklyReportHour { get; set; } = 3; // 3:00 AM
    
    /// <summary>
    /// Indica si se deben generar reportes mensuales
    /// </summary>
    public bool GenerateMonthlyReports { get; set; } = true;
    
    /// <summary>
    /// Día del mes para la generación de reportes mensuales (1-31)
    /// </summary>
    public int MonthlyReportDay { get; set; } = 1; // Primer día del mes
    
    /// <summary>
    /// Hora para la generación de reportes mensuales (0-23)
    /// </summary>
    public int MonthlyReportHour { get; set; } = 4; // 4:00 AM
} 