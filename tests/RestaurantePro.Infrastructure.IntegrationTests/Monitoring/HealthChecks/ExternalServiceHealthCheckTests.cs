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
using Moq.Contrib.HttpClient;

namespace RestaurantePro.Infrastructure.IntegrationTests.Monitoring.HealthChecks
{
    public class ExternalServiceHealthCheckTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<ExternalServiceHealthCheck>> _mockLogger;

        public ExternalServiceHealthCheckTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = _handlerMock.CreateClient();
            _mockLogger = new Mock<ILogger<ExternalServiceHealthCheck>>();
        }

        private ExternalServiceHealthCheck CreateHealthCheck(ExternalServiceHealthCheckOptions options)
        {
            var optionsWrapper = Options.Create(options);
            return new ExternalServiceHealthCheck(_httpClient, _mockLogger.Object, optionsWrapper);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenAllServicesAreHealthy_ShouldReturnHealthy()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "ServiceA", HealthCheckUrl = "http://service-a/health" },
                    new ExternalServiceConfig { Name = "ServiceB", HealthCheckUrl = "http://service-b/health" }
                }
            };
            _handlerMock.SetupRequest(HttpMethod.Get, "http://service-a/health").ReturnsResponse(HttpStatusCode.OK);
            _handlerMock.SetupRequest(HttpMethod.Get, "http://service-b/health").ReturnsResponse(HttpStatusCode.OK);

            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
            result.Description.Should().Be("Todos los servicios externos están disponibles");
            result.Data.Count.Should().Be(2);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenNonCriticalServiceFails_ShouldReturnDegraded()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "HealthyService", HealthCheckUrl = "http://healthy/health" },
                    new ExternalServiceConfig { Name = "FailingService", HealthCheckUrl = "http://failing/health", IsCritical = false }
                }
            };
            _handlerMock.SetupRequest(HttpMethod.Get, "http://healthy/health").ReturnsResponse(HttpStatusCode.OK);
            _handlerMock.SetupRequest(HttpMethod.Get, "http://failing/health").ReturnsResponse(HttpStatusCode.InternalServerError);

            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Degraded);
            result.Description.Should().Be("Servicio no crítico FailingService no disponible");
        }

        [Fact]
        public async Task CheckHealthAsync_WhenCriticalServiceFails_ShouldReturnUnhealthy()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "HealthyService", HealthCheckUrl = "http://healthy/health" },
                    new ExternalServiceConfig { Name = "FailingCriticalService", HealthCheckUrl = "http://failing-critical/health", IsCritical = true }
                }
            };
            _handlerMock.SetupRequest(HttpMethod.Get, "http://healthy/health").ReturnsResponse(HttpStatusCode.OK);
            _handlerMock.SetupRequest(HttpMethod.Get, "http://failing-critical/health").ReturnsResponse(HttpStatusCode.ServiceUnavailable);

            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be("Servicio crítico FailingCriticalService no disponible");
        }
        
        [Fact]
        public async Task CheckHealthAsync_WhenNonCriticalServiceTimesOut_ShouldReturnDegraded()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "TimeoutService", HealthCheckUrl = "http://timeout/health", TimeoutMs = 10 }
                }
            };
            _handlerMock.SetupRequest(HttpMethod.Get, "http://timeout/health")
                        .ThrowsAsync(new TaskCanceledException());
            
            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Degraded);
            result.Description.Should().Be("Timeout en servicio no crítico TimeoutService");
        }

        [Fact]
        public async Task CheckHealthAsync_WhenCriticalServiceTimesOut_ShouldReturnUnhealthy()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "CriticalTimeout", HealthCheckUrl = "http://timeout-critical/health", TimeoutMs = 10, IsCritical = true }
                }
            };
             _handlerMock.SetupRequest(HttpMethod.Get, "http://timeout-critical/health")
                        .ThrowsAsync(new TaskCanceledException());

            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be("Timeout en servicio crítico CriticalTimeout");
        }

        [Fact]
        public async Task CheckHealthAsync_WhenRequestThrowsException_ShouldReturnUnhealthyForCritical()
        {
            // Arrange
            var options = new ExternalServiceHealthCheckOptions
            {
                ServicesToCheck = new List<ExternalServiceConfig>
                {
                    new ExternalServiceConfig { Name = "ErrorService", HealthCheckUrl = "http://error/health", IsCritical = true }
                }
            };
            var exception = new HttpRequestException("Network error");
            _handlerMock.SetupRequest(HttpMethod.Get, "http://error/health").Throws(exception);

            var healthCheck = CreateHealthCheck(options);

            // Act
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be($"Error en servicio crítico ErrorService: {exception.Message}");
        }
    }
} 