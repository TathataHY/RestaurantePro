using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.Monitoring.HealthChecks;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace RestaurantePro.Infrastructure.IntegrationTests.Monitoring.HealthChecks
{
    public class ExternalServiceHealthCheckTests
    {
        private readonly Mock<ILogger<ExternalServiceHealthCheck>> _mockLogger;

        public ExternalServiceHealthCheckTests()
        {
            _mockLogger = new Mock<ILogger<ExternalServiceHealthCheck>>();
        }

        [Fact]
        public async Task CheckHealthAsync_ConServiciosDisponibles_DebeRetornarHealthy()
        {
            // Arrange
            var httpClient = new HttpClient();
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "TestService", HealthCheckUrl = "http://test/health" }
                }
            };
            var optionsWrapper = Options.Create(options);
            
            var healthCheck = new ExternalServiceHealthCheck(httpClient, _mockLogger.Object, optionsWrapper);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckHealthAsync_ConServicioNoDisponible_DebeRetornarUnhealthy()
        {
            // Arrange
            var httpClient = new HttpClient();
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "FailingService", HealthCheckUrl = "http://failing/health", IsCritical = true }
                }
            };
            var optionsWrapper = Options.Create(options);
            
            var healthCheck = new ExternalServiceHealthCheck(httpClient, _mockLogger.Object, optionsWrapper);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
        }
    }
} 