using System;
using System.Collections.Generic;
using Xunit;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using FluentAssertions;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
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
            var stats = telemetry.GetStatistics();
            stats["TotalHits"].Should().Be(2L);
        }
        
        [Fact]
        public void InMemoryCacheTelemetry_ShouldTrackMisses()
        {
            // Arrange
            var telemetry = new InMemoryCacheTelemetry();
            
            // Act
            telemetry.TrackCacheAccess("test_key2", false, "Get", 15);
            
            // Assert
            var stats = telemetry.GetStatistics();
            stats["TotalMisses"].Should().Be(1L);
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
            var stats = telemetry.GetStatistics();
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