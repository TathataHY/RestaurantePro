using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Core.Base;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
{
    public class CacheInvalidationEventHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly CacheInvalidationEventHandler _handler;
        
        public CacheInvalidationEventHandlerTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _handler = new CacheInvalidationEventHandler(_cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task Handle_DomainEvent_ShouldInvalidateProductoCategoriaServiceCache()
        {
            // Arrange
            var evento = new TestProductoEvent();
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern("ProductoCategoriaService_"),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_DomainEvent_ShouldInvalidateServicioNotificacionesCache()
        {
            // Arrange
            var evento = new TestNotificacionEvent();
            
            // Act
            await _handler.Handle(evento, CancellationToken.None);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern("ServicioNotificaciones_"),
                Times.Once);
        }
        
        [Fact]
        public async Task Handle_NullEvent_ShouldNotInvalidateCache()
        {
            // Act
            await _handler.Handle(null, CancellationToken.None);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.IsAny<string>()),
                Times.Never);
        }
        
        // Eventos de prueba
        private class TestProductoEvent : DomainEvent
        {
            public Guid ProductoId { get; } = Guid.NewGuid();
            public string Nombre { get; } = "Test Producto";
            
            public TestProductoEvent() : base() { }
        }
        
        private class TestNotificacionEvent : DomainEvent
        {
            public Guid NotificacionId { get; } = Guid.NewGuid();
            public string Titulo { get; } = "Test Notificación";
            
            public TestNotificacionEvent() : base() { }
        }
    }
} 