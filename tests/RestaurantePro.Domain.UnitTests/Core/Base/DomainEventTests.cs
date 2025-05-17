namespace RestaurantePro.Domain.UnitTests.Core.Base
{
    public class DomainEventTests
    {
        private class TestDomainEvent : DomainEvent
        {
            // OccurredOn se hereda de la clase base DomainEvent
        }

        [Fact]
        public void DomainEvent_ShouldHaveOccurredOnProperty()
        {
            // Arrange & Act
            var domainEvent = new TestDomainEvent();

            // Assert
            // Verificamos que OccurredOn sea cercano a la hora actual
            domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
} 