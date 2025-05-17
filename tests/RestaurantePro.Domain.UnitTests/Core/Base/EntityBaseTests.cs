namespace RestaurantePro.Domain.UnitTests.Core.Base
{
    public class EntityBaseTests
    {
        private class TestEntity : EntityBase
        {
            public TestEntity()
            {
            }

            public TestEntity(Guid id)
            {
                Id = id;
            }

            public void AddTestDomainEvent(IDomainEvent domainEvent)
            {
                AddDomainEvent(domainEvent);
            }

            public void ModifyEntity()
            {
                MarkAsModified();
            }
        }

        private class TestDomainEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; }

            public TestDomainEvent()
            {
                OccurredOn = DateTime.UtcNow;
            }
        }

        [Fact]
        public void NewEntity_ShouldHaveId()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            entity.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void NewEntity_ShouldHaveFechaCreacion()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            entity.FechaCreacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void NewEntity_ShouldNotBeDeleted()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            entity.EstaEliminado.Should().BeFalse();
        }

        [Fact]
        public void WhenMarkAsDeleted_ShouldSetEstaEliminadoToTrue()
        {
            // Arrange
            var entity = new TestEntity();

            // Act
            entity.MarkAsDeleted();

            // Assert
            entity.EstaEliminado.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkAsDeleted_ShouldUpdateFechaActualizacion()
        {
            // Arrange
            var entity = new TestEntity();

            // Act
            entity.MarkAsDeleted();

            // Assert
            entity.FechaActualizacion.Should().NotBeNull();
            entity.FechaActualizacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void WhenMarkAsModified_ShouldUpdateFechaActualizacion()
        {
            // Arrange
            var entity = new TestEntity();
            entity.FechaActualizacion.Should().BeNull();

            // Act
            entity.ModifyEntity();

            // Assert
            entity.FechaActualizacion.Should().NotBeNull();
            entity.FechaActualizacion.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void WhenAddDomainEvent_ShouldAddEventToCollection()
        {
            // Arrange
            var entity = new TestEntity();
            var domainEvent = new TestDomainEvent();

            // Act
            entity.AddTestDomainEvent(domainEvent);

            // Assert
            entity.DomainEvents.Should().ContainSingle();
            entity.DomainEvents.First().Should().BeSameAs(domainEvent);
        }

        [Fact]
        public void WhenClearDomainEvents_ShouldRemoveAllEvents()
        {
            // Arrange
            var entity = new TestEntity();
            entity.AddTestDomainEvent(new TestDomainEvent());
            entity.AddTestDomainEvent(new TestDomainEvent());

            // Act
            entity.ClearDomainEvents();

            // Assert
            entity.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenSameReference()
        {
            // Arrange
            var entity = new TestEntity();

            // Act & Assert
            entity.Equals(entity).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenDifferentType()
        {
            // Arrange
            var entity = new TestEntity();
            var otherObject = new object();

            // Act & Assert
            entity.Equals(otherObject).Should().BeFalse();
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenSameId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            // Act & Assert
            entity1.Equals(entity2).Should().BeTrue();
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenDifferentId()
        {
            // Arrange
            var entity1 = new TestEntity(Guid.NewGuid());
            var entity2 = new TestEntity(Guid.NewGuid());

            // Act & Assert
            entity1.Equals(entity2).Should().BeFalse();
        }

        [Fact]
        public void EqualityOperator_ShouldReturnTrue_WhenSameId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity1 = new TestEntity(id);
            var entity2 = new TestEntity(id);

            // Act & Assert
            (entity1 == entity2).Should().BeTrue();
        }

        [Fact]
        public void InequalityOperator_ShouldReturnTrue_WhenDifferentId()
        {
            // Arrange
            var entity1 = new TestEntity(Guid.NewGuid());
            var entity2 = new TestEntity(Guid.NewGuid());

            // Act & Assert
            (entity1 != entity2).Should().BeTrue();
        }
    }
}
