using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Decorators;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services.Cache.Telemetry
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
            var metrics = telemetry.GetMetrics();
            metrics.TotalHits.Should().Be(2);
            metrics.TotalAccesses.Should().Be(3);
            metrics.HitRate.Should().Be(2.0 / 3.0);
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
            var metrics = telemetry.GetMetrics();
            metrics.RecentInvalidations.Should().Be(2);
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
            var metrics = telemetry.GetMetrics();
            metrics.RecentErrors.Should().HaveCount(1);
            metrics.RecentErrors[0].Should().Contain("Test error");
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
            
            // Simulamos el comportamiento real de GetOrAddAsync, que busca primero si existe 
            // en caché y luego invoca la función loadFunc si no lo encuentra
            innerCacheMock.Setup(c => c.GetOrAddAsync<string>(
                    It.IsAny<string>(), 
                    It.IsAny<Func<CancellationToken, Task<string>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, Func<CancellationToken, Task<string>> loadFunc, int ttl, CancellationToken ct) => {
                    // Simulamos la implementación interna que usa el valor cargado y lo retorna
                    return "loaded_value"; 
                });
            
            var loadFunc = new Func<CancellationToken, Task<string>>(ct => Task.FromResult("loaded_value"));
            
            var decorator = new TelemetryCacheDecorator(innerCacheMock.Object, telemetryMock.Object);
            
            // Act
            var result = await decorator.GetOrAddAsync("key", loadFunc, 10);
            
            // Assert
            // Verificamos que se llamó al método correcto del caché interno
            innerCacheMock.Verify(
                c => c.GetOrAddAsync(
                    "key", 
                    It.IsAny<Func<CancellationToken, Task<string>>>(),
                    10, 
                    It.IsAny<CancellationToken>()
                ), 
                Times.Once
            );
            
            // Verificamos que la telemetría registró acceso al caché
            telemetryMock.Verify(t => t.TrackCacheAccess("key", true, "GetOrAddAsync", It.IsAny<long>()), Times.Once);
            
            // Verificamos que el resultado es el esperado
            result.Should().Be("loaded_value");
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
            var report = telemetry.GenerarInformeTelemetria();
            
            // Assert
            report.Should().Contain("INFORME DE TELEMETRÍA DE CACHÉ");
            report.Should().Contain("Total de accesos: 2");
            report.Should().Contain("Total de aciertos: 1");
            report.Should().Contain("Tasa de aciertos: 50.00%");
        }

        [Fact]
        public void GenerarInformeTelemetria_ShouldGenerateReadableReport()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            telemetry.TrackCacheAccess("test_key1", true, "Get", 10);
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            telemetry.TrackCacheInvalidation("pattern1", 3, 5);
            
            // Act
            var informe = telemetry.GenerarInformeTelemetria();
            
            // Assert
            informe.Should().Contain("INFORME DE TELEMETRÍA DE CACHÉ");
            informe.Should().Contain("Total de accesos: 2");
            informe.Should().Contain("Total de aciertos: 1");
            informe.Should().Contain("Tasa de aciertos: 50.00%");
        }
    }
} 