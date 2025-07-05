using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.Logging.Enrichers;
using RestaurantePro.Infrastructure.Logging.Providers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.DependencyInjection
{
    public class LoggingSetupTests
    {
        private readonly ServiceProvider _serviceProvider;

        public LoggingSetupTests()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLoggingServices(configuration);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddLoggingServices_ShouldRegisterLoggingProviders()
        {
            // Act
            var providers = _serviceProvider.GetServices<ILoggingProvider>().ToList();

            // Assert
            Assert.NotNull(providers);
            Assert.Equal(2, providers.Count);
            Assert.Contains(providers, p => p.GetType() == typeof(SerilogProvider));
            Assert.Contains(providers, p => p.GetType() == typeof(ApplicationInsightsProvider));
        }
        
        [Fact]
        public void AddLoggingServices_ShouldRegisterEnrichers()
        {
            // Act
            var userEnricher = _serviceProvider.GetService<UserEnricher>();
            var correlationEnricher = _serviceProvider.GetService<CorrelationEnricher>();
            var contextEnricher = _serviceProvider.GetService<ContextEnricher>();

            // Assert
            Assert.NotNull(userEnricher);
            Assert.NotNull(correlationEnricher);
            Assert.NotNull(contextEnricher);
        }

        [Fact]
        public void AddLoggingServices_ShouldRegisterCorrelationService()
        {
            // Act
            var correlationService = _serviceProvider.GetService<ICorrelationService>();

            // Assert
            Assert.NotNull(correlationService);
        }
    }
} 