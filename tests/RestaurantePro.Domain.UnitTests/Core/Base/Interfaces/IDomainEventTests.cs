using RestaurantePro.Domain.Core.Base.Interfaces;

namespace RestaurantePro.Domain.UnitTests.Core.Base.Interfaces
{
    public class IDomainEventTests
    {
        private class TestDomainEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; }

            public TestDomainEvent(DateTime occurredOn)
            {
                OccurredOn = occurredOn;
            }
        }

        [Fact]
        public void DomainEvent_ShouldHaveOccurredOnProperty()
        {
            // Arrange
            var occurredOn = DateTime.UtcNow;
            
            // Act
            var domainEvent = new TestDomainEvent(occurredOn);
            
            // Assert
            domainEvent.OccurredOn.Should().Be(occurredOn);
        }
    }
} 