using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Monitoring.HealthChecks;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;

namespace RestaurantePro.Infrastructure.IntegrationTests.Monitoring.HealthChecks
{
    public class CacheHealthCheckTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<CacheHealthCheck>> _mockLogger;
        private readonly CacheHealthCheck _healthCheck;

        public CacheHealthCheckTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<CacheHealthCheck>>();
            _healthCheck = new CacheHealthCheck(_mockCacheService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenCacheIsWorking_ShouldReturnHealthy()
        {
            // Arrange
            const string testKey = "HealthCheck_Test_Key";
            const string testValue = "Cache is working!";

            _mockCacheService.Setup(s => s.SetAsync(testKey, testValue, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _mockCacheService.Setup(s => s.GetAsync<string>(testKey, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(testValue);
            _mockCacheService.Setup(s => s.RemoveAsync(testKey, It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
            result.Description.Should().Be("Servicio de caché disponible y funcionando correctamente");
            _mockCacheService.Verify(s => s.SetAsync(testKey, testValue, 1, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(s => s.GetAsync<string>(testKey, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(s => s.RemoveAsync(testKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenCacheIsInconsistent_ShouldReturnDegraded()
        {
            // Arrange
            const string testKey = "HealthCheck_Test_Key";
            const string testValue = "Cache is working!";
            const string wrongValue = "Wrong value!";

            _mockCacheService.Setup(s => s.SetAsync(testKey, testValue, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);
            _mockCacheService.Setup(s => s.GetAsync<string>(testKey, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(wrongValue);

            // Act
            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Degraded);
            result.Description.Should().Be("El servicio de caché no está funcionando correctamente: inconsistencia en lectura/escritura");
        }

        [Fact]
        public async Task CheckHealthAsync_WhenCacheThrowsException_ShouldReturnUnhealthy()
        {
            // Arrange
            var exception = new Exception("Cache is down");
            _mockCacheService.Setup(s => s.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                           .ThrowsAsync(exception);

            // Act
            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
            result.Description.Should().Be($"Error en el servicio de caché: {exception.Message}");
            result.Exception.Should().Be(exception);
        }

        [Fact]
        public async Task CheckHealthAsync_WhenStatisticsProviderIsAvailable_ShouldReturnData()
        {
            // Arrange
            var mockStatsProvider = new Mock<ICacheService>();
            var statsProvider = mockStatsProvider.As<IProvidesCacheStatistics>();
            var stats = new Dictionary<string, object> { { "Hits", 100 }, { "Misses", 5 } };
            statsProvider.Setup(p => p.GetStatisticsAsync()).ReturnsAsync(stats);

            const string testKey = "HealthCheck_Test_Key";
            const string testValue = "Cache is working!";
            mockStatsProvider.Setup(s => s.GetAsync<string>(testKey, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(testValue);

            var healthCheckWithStats = new CacheHealthCheck(mockStatsProvider.Object, _mockLogger.Object);
            
            // Act
            var result = await healthCheckWithStats.CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
            result.Data.Should().ContainKey("Hits").And.ContainValue(100);
            result.Data.Should().ContainKey("Misses").And.ContainValue(5);
        }
    }
} 