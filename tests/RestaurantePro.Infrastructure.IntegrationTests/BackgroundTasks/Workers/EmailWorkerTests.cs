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
    public class EmailWorkerTests
    {
        private readonly IEmailQueueProcessor _mockEmailProcessor;
        private readonly ILogger<EmailWorker> _mockLogger;

        public EmailWorkerTests()
        {
            _mockEmailProcessor = Substitute.For<IEmailQueueProcessor>();
            _mockLogger = Substitute.For<ILogger<EmailWorker>>();
        }

        [Fact]
        public async Task EmailWorker_ShouldProcessEmails_WhenExecuted()
        {
            // Arrange
            var services = new ServiceCollection();
            
            services.Configure<EmailWorkerOptions>(options =>
            {
                options.ProcessingInterval = TimeSpan.FromMilliseconds(50);
                options.BatchSize = 10;
            });

            services.AddSingleton(_mockLogger);
            services.AddSingleton<IHostedService, EmailWorker>();
            
            // The worker creates a scope to resolve this service
            services.AddScoped<IEmailQueueProcessor>(sp => _mockEmailProcessor);

            var serviceProvider = services.BuildServiceProvider();
            var worker = serviceProvider.GetRequiredService<IHostedService>() as EmailWorker;

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await worker.StartAsync(cancellationTokenSource.Token);
            
            // Give the worker some time to execute
            await Task.Delay(200, cancellationTokenSource.Token);
            
            await worker.StopAsync(cancellationTokenSource.Token);

            // Assert
            await _mockEmailProcessor.Received().ProcessPendingEmailsAsync(Arg.Is(10), Arg.Any<CancellationToken>());
        }
        
        [Fact]
        public async Task EmailWorker_ShouldLogInformation_WhenStartingAndStopping()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<EmailWorkerOptions>>(Options.Create(new EmailWorkerOptions()));
            services.AddScoped<IEmailQueueProcessor>(sp => _mockEmailProcessor);
            var serviceProvider = services.BuildServiceProvider();

            var worker = new EmailWorker(_mockLogger, serviceProvider, Options.Create(new EmailWorkerOptions()));
            var cancellationToken = new CancellationToken();

            // Act
            await worker.StartAsync(cancellationToken);
            await worker.StopAsync(cancellationToken);

            // Assert
            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Servicio de procesamiento de emails iniciando")),
                null,
                Arg.Any<Func<object, Exception, string>>());

            _mockLogger.Received(1).Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(o => o.ToString().Contains("Servicio de procesamiento de emails detenido")),
                null,
                Arg.Any<Func<object, Exception, string>>());
        }
    }
} 