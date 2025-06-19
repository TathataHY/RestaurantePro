using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Interfaces.Services;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Infrastructure.BackgroundTasks.Workers;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Workers
{
    public class ReportGenerationWorkerTests
    {
        private readonly IReportQueueProcessor _mockReportQueueProcessor;
        private readonly IReportService _mockReportService;
        private readonly ILogger<ReportGenerationWorker> _mockLogger;

        public ReportGenerationWorkerTests()
        {
            _mockReportQueueProcessor = Substitute.For<IReportQueueProcessor>();
            _mockReportService = Substitute.For<IReportService>();
            _mockLogger = Substitute.For<ILogger<ReportGenerationWorker>>();
            
            // Setup default return values for mocked services to avoid null reference issues
            _mockReportService.GenerarReporteDiarioVentasAsync(default, default)
                .ReturnsForAnyArgs(new ReportInfo { Id = Guid.NewGuid() });
        }

        [Fact]
        public async Task ReportGenerationWorker_ShouldProcessQueuedReports_WhenExecuted()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.Configure<ReportGenerationWorkerOptions>(options =>
            {
                options.CheckInterval = TimeSpan.FromMilliseconds(50);
                options.BatchSize = 5;
                // Disable scheduled reports to isolate queue processing
                options.ReportGenerationStartHour = 0;
                options.ReportGenerationEndHour = 0;
            });

            services.AddSingleton(_mockLogger);
            services.AddSingleton<IHostedService, ReportGenerationWorker>();
            
            services.AddScoped<IReportQueueProcessor>(sp => _mockReportQueueProcessor);
            services.AddScoped<IReportService>(sp => _mockReportService);

            var serviceProvider = services.BuildServiceProvider();
            var worker = serviceProvider.GetRequiredService<IHostedService>() as ReportGenerationWorker;

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await worker.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(200, cancellationTokenSource.Token);
            await worker.StopAsync(cancellationTokenSource.Token);

            // Assert
            await _mockReportQueueProcessor.Received().ProcessPendingReportsAsync(Arg.Is(5), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReportGenerationWorker_ShouldNotGenerateScheduledReports_WhenTimeIsNotRight()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.Configure<ReportGenerationWorkerOptions>(options =>
            {
                options.CheckInterval = TimeSpan.FromMilliseconds(50);
                // Set generation window to a time that won't be hit (e.g., midnight)
                // This assumes tests are not run exactly at midnight.
                options.ReportGenerationStartHour = 0;
                options.ReportGenerationEndHour = 0;
            });

            services.AddSingleton(_mockLogger);
            services.AddSingleton<IHostedService, ReportGenerationWorker>();
            
            services.AddScoped<IReportQueueProcessor>(sp => _mockReportQueueProcessor);
            services.AddScoped<IReportService>(sp => _mockReportService);

            var serviceProvider = services.BuildServiceProvider();
            var worker = serviceProvider.GetRequiredService<IHostedService>() as ReportGenerationWorker;

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await worker.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(200, cancellationTokenSource.Token);
            await worker.StopAsync(cancellationTokenSource.Token);

            // Assert
            await _mockReportService.DidNotReceiveWithAnyArgs().GenerarReporteDiarioVentasAsync(default, default);
            await _mockReportService.DidNotReceiveWithAnyArgs().GenerarReporteSemanalVentasAsync(default, default, default);
            await _mockReportService.DidNotReceiveWithAnyArgs().GenerarReporteMensualVentasAsync(default, default, default);
        }
    }
} 