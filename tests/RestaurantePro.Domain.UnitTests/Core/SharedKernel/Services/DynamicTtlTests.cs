using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
{
    public class DynamicTtlTests
    {
        [Fact]
        public void UsageBasedTtlStrategy_ShouldReturnDefaultTtl_ForNewKey()
        {
            // Arrange
            var strategy = new UsageBasedTtlStrategy();
            const int defaultTtl = 60;
            
            // Act
            int calculatedTtl = strategy.CalculateTtl("new_key", defaultTtl);
            
            // Assert
            calculatedTtl.Should().Be(defaultTtl);
        }
        
        [Fact]
        public void UsageBasedTtlStrategy_ShouldIncreaseTtl_ForFrequentlyAccessedKey()
        {
            // Arrange
            var strategy = new UsageBasedTtlStrategy();
            const string key = "frequent_key";
            const int defaultTtl = 30;
            
            // Simular acceso frecuente
            for (int i = 0; i < 20; i++)
            {
                strategy.RegisterAccess(key, true, "Get");
            }
            
            // Act
            int calculatedTtl = strategy.CalculateTtl(key, defaultTtl);
            
            // Assert
            calculatedTtl.Should().BeGreaterThan(defaultTtl);
        }
        
        [Fact]
        public void UsageBasedTtlStrategy_ShouldDecreaseTtl_ForFrequentlyInvalidatedKey()
        {
            // Arrange
            var strategy = new UsageBasedTtlStrategy();
            const string key = "test_key";
            const string pattern = "test";
            const int defaultTtl = 60;
            
            // Simular algunos accesos
            for (int i = 0; i < 5; i++)
            {
                strategy.RegisterAccess(key, true, "Get");
            }
            
            // Simular invalidaciones frecuentes
            for (int i = 0; i < 3; i++)
            {
                strategy.RegisterInvalidation(pattern, 1);
            }
            
            // Act
            int calculatedTtl = strategy.CalculateTtl(key, defaultTtl);
            
            // Assert
            calculatedTtl.Should().NotBe(defaultTtl, "El TTL debería ser diferente debido a las invalidaciones frecuentes");
        }
        
        [Fact]
        public void UsageBasedTtlStrategy_ShouldIncreaseTtl_ForHighHitRate()
        {
            // Arrange
            var strategy = new UsageBasedTtlStrategy();
            const string key = "high_hit_rate_key";
            const int defaultTtl = 40;
            
            // Simular alta tasa de aciertos (90%)
            for (int i = 0; i < 9; i++)
            {
                strategy.RegisterAccess(key, true, "Get");
            }
            strategy.RegisterAccess(key, false, "Get");
            
            // Act
            int calculatedTtl = strategy.CalculateTtl(key, defaultTtl);
            
            // Assert
            calculatedTtl.Should().BeGreaterThan(defaultTtl);
        }
        
        [Fact]
        public void SmartCacheDecorator_ShouldUseDynamicTtl_ForSetOperations()
        {
            // Arrange
            var mockInnerCache = new Mock<ICacheService>();
            var mockTelemetry = new Mock<ICacheTelemetry>();
            var mockTtlStrategy = new Mock<IDynamicTtlStrategy>();
            
            const string key = "test_key";
            const string value = "test_value";
            const int defaultTtl = 30;
            const int dynamicTtl = 45;
            
            mockTtlStrategy
                .Setup(s => s.CalculateTtl(key, defaultTtl))
                .Returns(dynamicTtl);
                
            var smartCache = new SmartCacheDecorator(
                mockInnerCache.Object,
                mockTelemetry.Object,
                mockTtlStrategy.Object);
                
            // Act
            smartCache.Set(key, value, defaultTtl);
            
            // Assert
            mockTtlStrategy.Verify(s => s.CalculateTtl(key, defaultTtl), Times.Once);
            mockInnerCache.Verify(c => c.Set(key, value, dynamicTtl), Times.Once);
        }
        
        [Fact]
        public void SmartCacheDecorator_ShouldRegisterAccess_ForGetOperations()
        {
            // Arrange
            var mockInnerCache = new Mock<ICacheService>();
            var mockTelemetry = new Mock<ICacheTelemetry>();
            var mockTtlStrategy = new Mock<IDynamicTtlStrategy>();
            
            const string key = "test_key";
            const string value = "test_value";
            
            mockInnerCache
                .Setup(c => c.Get<string>(key))
                .Returns(value);
                
            var smartCache = new SmartCacheDecorator(
                mockInnerCache.Object,
                mockTelemetry.Object,
                mockTtlStrategy.Object);
                
            // Act
            var result = smartCache.Get<string>(key);
            
            // Assert
            result.Should().Be(value);
            mockTtlStrategy.Verify(s => s.RegisterAccess(key, true, "Get"), Times.Once);
            mockTelemetry.Verify(t => t.TrackCacheAccess(key, true, "Get", It.IsAny<long>()), Times.Once);
        }
        
        [Fact]
        public async Task SmartCacheDecorator_ShouldUseDynamicTtl_ForAsyncOperations()
        {
            // Arrange
            var mockInnerCache = new Mock<ICacheService>();
            var mockTelemetry = new Mock<ICacheTelemetry>();
            var mockTtlStrategy = new Mock<IDynamicTtlStrategy>();
            
            const string key = "async_key";
            const string value = "async_value";
            const int defaultTtl = 20;
            const int dynamicTtl = 35;
            
            mockInnerCache
                .Setup(c => c.Exists(key))
                .Returns(false);
                
            mockTtlStrategy
                .Setup(s => s.CalculateTtl(key, defaultTtl))
                .Returns(dynamicTtl);
                
            var smartCache = new SmartCacheDecorator(
                mockInnerCache.Object,
                mockTelemetry.Object,
                mockTtlStrategy.Object);
                
            // Act
            var result = await smartCache.GetOrAddAsync(
                key,
                ct => Task.FromResult(value),
                defaultTtl);
                
            // Assert
            result.Should().Be(value);
            mockTtlStrategy.Verify(s => s.CalculateTtl(key, defaultTtl), Times.Once);
            mockInnerCache.Verify(c => c.Set(key, value, dynamicTtl), Times.Once);
            mockTtlStrategy.Verify(s => s.RegisterAccess(key, false, "GetOrAddAsync"), Times.Once);
        }
        
        [Fact]
        public void SmartCacheDecorator_ShouldRegisterInvalidation_ForInvalidatePattern()
        {
            // Arrange
            var mockInnerCache = new Mock<ICacheService>();
            var mockTelemetry = new Mock<ICacheTelemetry>();
            var mockTtlStrategy = new Mock<IDynamicTtlStrategy>();
            
            const string pattern = "test_pattern";
            
            var smartCache = new SmartCacheDecorator(
                mockInnerCache.Object,
                mockTelemetry.Object,
                mockTtlStrategy.Object);
                
            // Act
            smartCache.InvalidatePattern(pattern);
            
            // Assert
            mockInnerCache.Verify(c => c.InvalidatePattern(pattern), Times.Once);
            mockTtlStrategy.Verify(s => s.RegisterInvalidation(pattern, It.IsAny<int>()), Times.Once);
            mockTelemetry.Verify(t => t.TrackCacheInvalidation(pattern, It.IsAny<int>(), It.IsAny<long>()), Times.Once);
        }
    }
} 