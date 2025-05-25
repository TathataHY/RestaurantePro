namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services.Cache.Invalidation
{
    public class CacheInvalidationTests
    {
        [Fact]
        public async Task EventHandler_ShouldInvalidateCache_WhenEventOccurs()
        {
            // Arrange
            var cacheServiceMock = new Mock<ICacheService>();
            var handler = new CacheInvalidationEventHandler(cacheServiceMock.Object);
            var testEvent = new TestDomainEvent();
            
            // Act
            await handler.Handle(testEvent, CancellationToken.None);
            
            // Assert
            // Verificar que el patrón de caché correcto se ha invalidado
            cacheServiceMock.Verify(c => c.InvalidatePattern("ProductoCategoriaService_"), Times.Once);
        }
        
        [Fact]
        public void Extensions_ShouldExtractEntityId_FromEvent()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            var testEvent = new TestEntityEvent(expectedId);
            
            // Act
            var result = CacheInvalidationExtensions.GetEntityId(testEvent);
            
            // Assert
            Assert.Equal(expectedId, result);
        }
        
        private class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent() : base() { }
        }
        
        private class TestEntityEvent : DomainEvent
        {
            public Guid EntityId { get; }
            
            public TestEntityEvent(Guid entityId) : base()
            {
                EntityId = entityId;
            }
        }
    }
} 