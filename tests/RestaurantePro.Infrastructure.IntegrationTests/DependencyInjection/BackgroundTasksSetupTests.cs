using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Interfaces.Services;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Interfaces;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Comercial;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Core;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Inventario;
using RestaurantePro.Infrastructure.BackgroundTasks.Jobs.Operaciones;
using RestaurantePro.Infrastructure.BackgroundTasks.Schedulers;
using RestaurantePro.Infrastructure.BackgroundTasks.Workers;
using RestaurantePro.Infrastructure.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Hangfire;
using Hangfire.Storage;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class BackgroundTasksSetupTests
    {
        private ServiceProvider _serviceProvider;

        private void Setup(Dictionary<string, string> config = null)
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config ?? new Dictionary<string, string>())
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            
            // Llamar al método de extensión con isTestEnvironment = true
            services.AddBackgroundTasksServices(configuration, isTestEnvironment: true);

            // Mocks para Jobs y Workers
            services.AddSingleton(Substitute.For<INotificacionRepository>());
            services.AddSingleton(Substitute.For<IUnitOfWork>());
            services.AddSingleton(Substitute.For<IUsuarioRepository>());
            services.AddSingleton(Substitute.For<IEmailService>());
            services.AddSingleton(Substitute.For<IFacturaRepository>());
            services.AddSingleton(Substitute.For<IComandaRepository>());
            services.AddSingleton(Substitute.For<IReservacionRepository>());
            services.AddSingleton(Substitute.For<IIngredienteRepository>());
            services.AddSingleton(Substitute.For<INotificationService>());
            services.AddSingleton(Substitute.For<IMesaRepository>());

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddBackgroundTasksServices_ShouldRegisterJobScheduler()
        {
            // Arrange
            Setup();

            // Act
            var scheduler = _serviceProvider.GetService<IJobScheduler>();

            // Assert
            Assert.NotNull(scheduler);
            Assert.IsType<HangfireScheduler>(scheduler);
        }

        [Fact]
        public void AddBackgroundTasksServices_ShouldRegisterOptions()
        {
            // Arrange
            var settings = new Dictionary<string, string>
            {
                { "BackgroundTasks:NotificationCleanup:DaysToKeep", "15" },
                { "BackgroundTasks:UserInactivity:InactivityThresholdDays", "30" },
                { "BackgroundTasks:LowStockAlert:CriticalThresholdPercentage", "50" },
                { "BackgroundTasks:NotificationWorker:ProcessingInterval", "00:00:05" },
                { "BackgroundTasks:EmailWorker:BatchSize", "20" },
                { "BackgroundTasks:ReportGenerationWorker:CheckInterval", "00:15:00" }
            };
            Setup(settings);

            // Act
            var notificationOptions = _serviceProvider.GetService<IOptions<NotificationCleanupOptions>>();
            var userInactivityOptions = _serviceProvider.GetService<IOptions<UserInactivityOptions>>();
            var lowStockOptions = _serviceProvider.GetService<IOptions<LowStockAlertOptions>>();
            var notificationWorkerOptions = _serviceProvider.GetService<IOptions<NotificationWorkerOptions>>();
            var emailWorkerOptions = _serviceProvider.GetService<IOptions<EmailWorkerOptions>>();
            var reportWorkerOptions = _serviceProvider.GetService<IOptions<ReportGenerationWorkerOptions>>();
            
            // Assert
            Assert.NotNull(notificationOptions);
            Assert.Equal(15, notificationOptions.Value.DaysToKeep);

            Assert.NotNull(userInactivityOptions);
            Assert.Equal(30, userInactivityOptions.Value.InactivityThresholdDays);
            
            Assert.NotNull(lowStockOptions);
            Assert.Equal(50, lowStockOptions.Value.CriticalThresholdPercentage);

            Assert.NotNull(notificationWorkerOptions);
            Assert.Equal(TimeSpan.FromSeconds(5), notificationWorkerOptions.Value.ProcessingInterval);

            Assert.NotNull(emailWorkerOptions);
            Assert.Equal(20, emailWorkerOptions.Value.BatchSize);

            Assert.NotNull(reportWorkerOptions);
            Assert.Equal(TimeSpan.FromMinutes(15), reportWorkerOptions.Value.CheckInterval);
        }

        [Fact]
        public void AddBackgroundTasksServices_ShouldRegisterJobs()
        {
            // Arrange
            Setup();

            // Act
            var cleanupJob = _serviceProvider.GetService<NotificationCleanupJob>();
            var inactivityJob = _serviceProvider.GetService<UserInactivityJob>();
            var invoiceJob = _serviceProvider.GetService<InvoiceReminderJob>();
            var tableJob = _serviceProvider.GetService<TableCleanupJob>();
            var stockJob = _serviceProvider.GetService<LowStockAlertJob>();
            var expirationJob = _serviceProvider.GetService<ExpirationCheckJob>();

            // Assert
            Assert.NotNull(cleanupJob);
            Assert.NotNull(inactivityJob);
            Assert.NotNull(invoiceJob);
            Assert.NotNull(tableJob);
            Assert.NotNull(stockJob);
            Assert.NotNull(expirationJob);
        }

        [Fact]
        public void AddBackgroundTasksServices_ShouldRegisterWorkersAsHostedServices()
        {
            // Arrange
            Setup();

            // Act
            var hostedServices = _serviceProvider.GetServices<IHostedService>().ToList();

            // Assert
            Assert.NotNull(hostedServices);
            Assert.True(hostedServices.Any(s => s.GetType() == typeof(EmailWorker)));
            Assert.True(hostedServices.Any(s => s.GetType() == typeof(NotificationWorker)));
            Assert.True(hostedServices.Any(s => s.GetType() == typeof(ReportGenerationWorker)));
        }
    }
} 