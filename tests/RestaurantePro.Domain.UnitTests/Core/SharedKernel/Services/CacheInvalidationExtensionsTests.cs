using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Core.Base;
using System;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
{
    public class CacheInvalidationExtensionsTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        
        public CacheInvalidationExtensionsTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
        }
        
        [Fact]
        public void GetEntityId_EventWithEntityIdProperty_ShouldReturnId()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            var evento = new TestProductoCreado(expectedId, "Test");
            
            // Act
            var result = CacheInvalidationExtensions.GetEntityId(evento);
            
            // Assert
            result.Should().Be(expectedId);
        }
        
        [Fact]
        public void GetEntityId_EventWithDifferentIdProperty_ShouldFindCorrectId()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            var evento = new TestEventWithDifferentIdName(expectedId, "Test");
            
            // Act
            var result = CacheInvalidationExtensions.GetEntityId(evento);
            
            // Assert
            result.Should().Be(expectedId);
        }
        
        [Fact]
        public void GetEntityId_EventWithNoId_ShouldReturnNull()
        {
            // Arrange
            var evento = new TestEventWithNoId("Test");
            
            // Act
            var result = CacheInvalidationExtensions.GetEntityId(evento);
            
            // Assert
            result.Should().Be(Guid.Empty);
        }
        
        [Fact]
        public void GetEntityId_NullEvent_ShouldReturnNull()
        {
            // Act
            var result = CacheInvalidationExtensions.GetEntityId(null);
            
            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public void InvalidateForEntity_ValidParameters_ShouldInvalidateCorrectPattern()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var servicePrefix = "TestService_";
            var expectedPattern = $"{servicePrefix}*{entityId}*";
            
            // Act
            _cacheServiceMock.Object.InvalidateForEntity(servicePrefix, entityId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == expectedPattern)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidateForEntity_NullCacheService_ShouldNotThrowException()
        {
            // Arrange
            ICacheService nullService = null;
            var entityId = Guid.NewGuid();
            var servicePrefix = "TestService_";
            
            // Act & Assert
            var action = new Action(() => nullService.InvalidateForEntity(servicePrefix, entityId));
            action.Should().NotThrow();
        }
        
        [Fact]
        public void InvalidateForEntity_EmptyPrefix_ShouldNotInvalidate()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var servicePrefix = "";
            
            // Act
            _cacheServiceMock.Object.InvalidateForEntity(servicePrefix, entityId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.IsAny<string>()),
                Times.Never);
        }
        
        [Fact]
        public void InvalidateForEvent_EventWithId_ShouldInvalidateEntityPattern()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var servicePrefix = "TestService_";
            var evento = new TestProductoCreado(entityId, "Test");
            var expectedPattern = $"{servicePrefix}*{entityId}*";
            
            // Act
            _cacheServiceMock.Object.InvalidateForEvent(servicePrefix, evento);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == expectedPattern)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidateForEvent_EventWithoutId_ShouldInvalidateServicePrefix()
        {
            // Arrange
            var servicePrefix = "TestService_";
            var evento = new TestEventWithNoId("Test");
            var expectedPattern = $"{servicePrefix}*{Guid.Empty}*";
            
            // Act
            _cacheServiceMock.Object.InvalidateForEvent(servicePrefix, evento);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == expectedPattern)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidateForEvent_NullEvent_ShouldNotInvalidate()
        {
            // Arrange
            var servicePrefix = "TestService_";
            
            // Act
            _cacheServiceMock.Object.InvalidateForEvent(servicePrefix, null);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.IsAny<string>()),
                Times.Never);
        }
        
        // Clases de prueba para eventos
        private class TestProductoCreado : DomainEvent
        {
            public Guid ProductoId { get; }
            public string Nombre { get; }
            
            public TestProductoCreado(Guid productoId, string nombre)
            {
                ProductoId = productoId;
                Nombre = nombre;
            }
        }
        
        private class TestEventWithDifferentIdName : DomainEvent
        {
            public Guid TestEntityId { get; }
            public string Nombre { get; }
            
            public TestEventWithDifferentIdName(Guid testEntityId, string nombre)
            {
                TestEntityId = testEntityId;
                Nombre = nombre;
            }
        }
        
        private class TestEventWithNoId : DomainEvent
        {
            public string Nombre { get; }
            
            public TestEventWithNoId(string nombre)
            {
                Nombre = nombre;
            }
        }
    }
} 