using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using FluentAssertions;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
{
    public class CacheTelemetryTests
    {
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackHitsAndMisses()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheAccess("test_key1", true, "Get", 10);
            telemetry.TrackCacheAccess("test_key1", true, "Get", 12);
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            
            // Assert
            var stats = telemetry.GetStatistics();
            stats["TotalHits"].Should().Be(2L);
            stats["TotalMisses"].Should().Be(1L);
            stats["HitRate"].Should().Be(2.0 / 3.0);
        }
        
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackInvalidations()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheInvalidation("pattern1", 5, 10);
            telemetry.TrackCacheInvalidation("pattern2", 3, 8);
            
            // Assert
            var stats = telemetry.GetStatistics();
            stats["TotalInvalidations"].Should().Be(2L);
            stats["TotalKeysAffected"].Should().Be(8L);
        }
        
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackErrors()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            var exception = new InvalidOperationException("Test error");
            
            // Act
            telemetry.TrackCacheError("test_key", "Get", exception);
            
            // Assert
            var stats = telemetry.GetStatistics();
            stats["TotalErrors"].Should().Be(1L);
            var errors = (List<dynamic>)stats["RecentErrors"];
            errors.Should().HaveCount(1);
            ((string)errors[0].Error).Should().Be("Test error");
        }
        
        [Fact]
        public void TelemetryCacheDecorator_ShouldTrackGetHits()
        {
            // Arrange
            var innerCacheMock = new Mock<ICacheService>();
            var telemetryMock = new Mock<ICacheTelemetry>();
            
            innerCacheMock.Setup(c => c.Get<string>("hit_key"))
                .Returns("cached_value");
                
            innerCacheMock.Setup(c => c.Get<string>("miss_key"))
                .Returns((string)null);
                
            var decorator = new TelemetryCacheDecorator(innerCacheMock.Object, telemetryMock.Object);
            
            // Act
            var hitResult = decorator.Get<string>("hit_key");
            var missResult = decorator.Get<string>("miss_key");
            
            // Assert
            telemetryMock.Verify(t => t.TrackCacheAccess("hit_key", true, "Get", It.IsAny<long>()), Times.Once);
            telemetryMock.Verify(t => t.TrackCacheAccess("miss_key", false, "Get", It.IsAny<long>()), Times.Once);
        }
        
        [Fact]
        public void TelemetryCacheDecorator_ShouldTrackErrors()
        {
            // Arrange
            var innerCacheMock = new Mock<ICacheService>();
            var telemetryMock = new Mock<ICacheTelemetry>();
            var exception = new InvalidOperationException("Test error");
            
            innerCacheMock.Setup(c => c.Get<string>("error_key"))
                .Throws(exception);
                
            var decorator = new TelemetryCacheDecorator(innerCacheMock.Object, telemetryMock.Object);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => decorator.Get<string>("error_key"));
            
            telemetryMock.Verify(t => t.TrackCacheError("error_key", "Get", exception), Times.Once);
        }
        
        [Fact]
        public async Task TelemetryCacheDecorator_ShouldTrackAsyncOperations()
        {
            // Arrange
            var innerCacheMock = new Mock<ICacheService>();
            var telemetryMock = new Mock<ICacheTelemetry>();
            
            innerCacheMock.Setup(c => c.Get<string>("key"))
                .Returns((string)null);
                
            var loadFunc = new Func<CancellationToken, Task<string>>(ct => Task.FromResult("loaded_value"));
            
            var decorator = new TelemetryCacheDecorator(innerCacheMock.Object, telemetryMock.Object);
            
            // Act
            var result = await decorator.GetOrAddAsync("key", loadFunc);
            
            // Assert
            innerCacheMock.Verify(c => c.Set("key", "loaded_value", It.IsAny<int>()), Times.Once);
            telemetryMock.Verify(t => t.TrackCacheAccess("key", false, "GetOrAddAsync", It.IsAny<long>()), Times.Once);
        }
        
        [Fact]
        public void GetHumanReadableReport_ShouldGenerateReadableReport()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            telemetry.TrackCacheAccess("test_key1", true, "Get", 10);
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            telemetry.TrackCacheInvalidation("pattern1", 3, 5);
            
            // Act
            var report = telemetry.GetHumanReadableReport();
            
            // Assert
            report.Should().Contain("INFORME DE TELEMETRÍA DE CACHÉ");
            report.Should().Contain("Total aciertos: 1");
            report.Should().Contain("Total fallos: 1");
            report.Should().Contain("Tasa de aciertos: 50.00%");
        }
    }
} 