using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces.Services;
using RestaurantePro.Infrastructure.BackgroundTasks.Workers;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.BackgroundTasks.Workers
{
    public class NotificationWorkerTests
    {
        private readonly INotificationProcessor _mockNotificationProcessor;
        private readonly ILogger<NotificationWorker> _mockLogger;

        public NotificationWorkerTests()
        {
            _mockNotificationProcessor = Substitute.For<INotificationProcessor>();
            _mockLogger = Substitute.For<ILogger<NotificationWorker>>();
        }

        [Fact]
        public async Task NotificationWorker_ShouldProcessNotifications_WhenExecuted()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.Configure<NotificationWorkerOptions>(options =>
            {
                options.ProcessingInterval = TimeSpan.FromMilliseconds(50);
                options.BatchSize = 20;
            });

            services.AddSingleton(_mockLogger);
            services.AddSingleton<IHostedService, NotificationWorker>();
            
            // The worker creates a scope to resolve this service
            services.AddScoped<INotificationProcessor>(sp => _mockNotificationProcessor);

            var serviceProvider = services.BuildServiceProvider();
            var worker = serviceProvider.GetRequiredService<IHostedService>() as NotificationWorker;

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await worker.StartAsync(cancellationTokenSource.Token);
            
            // Give the worker some time to execute
            await Task.Delay(200, cancellationTokenSource.Token);
            
            await worker.StopAsync(cancellationTokenSource.Token);

            // Assert
            // The current implementation calls the overload without batch size.
            // We verify that this specific overload is called.
            await _mockNotificationProcessor.Received().ProcessPendingNotificationsAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task NotificationWorker_ShouldLogInformation_WhenStarting()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<NotificationWorkerOptions>>(Options.Create(new NotificationWorkerOptions()));
            services.AddScoped<INotificationProcessor>(sp => _mockNotificationProcessor);
            var serviceProvider = services.BuildServiceProvider();

            var worker = new NotificationWorker(_mockLogger, serviceProvider, Options.Create(new NotificationWorkerOptions()));
            var cancellationToken = new CancellationToken();

            // Act
            await worker.StartAsync(cancellationToken);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Notification Worker iniciado")),
                null,
                Arg.Any<Func<object, Exception, string>>());
        }
    }
} 