namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services.Cache.Telemetry
{
    public class CacheTelemetryTestsTemp
    {
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackHits()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheAccess("test_key1", true, "Get", 10);
            telemetry.TrackCacheAccess("test_key1", true, "Get", 12);
            
            // Assert
            var metrics = telemetry.GetMetrics();
            metrics.TotalHits.Should().Be(2);
        }
        
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackMisses()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            
            // Assert
            var metrics = telemetry.GetMetrics();
            metrics.TotalAccesses.Should().Be(1);
        }
        
        [Fact]
        public void InMemoryCacheTelemetry_ShouldCalculateHitRate()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheAccess("test_key1", true, "Get", 10);
            telemetry.TrackCacheAccess("test_key1", true, "Get", 12);
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            
            // Assert
            var metrics = telemetry.GetMetrics();
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