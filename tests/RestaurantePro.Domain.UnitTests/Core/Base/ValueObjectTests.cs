using RestaurantePro.Domain.Core.Base;

namespace RestaurantePro.Domain.UnitTests.Core.Base
{
    public class ValueObjectTests
    {
        // Ejemplo de un Value Object que implementará la clase base ValueObject
        private class Dinero : ValueObject
        {
            public decimal Monto { get; private set; }
            public string Moneda { get; private set; }

            public Dinero(decimal monto, string moneda)
            {
                Monto = monto;
                Moneda = moneda;
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Monto;
                yield return Moneda;
            }
        }

        [Fact]
        public void ValueObjects_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            var dinero1 = new Dinero(100, "EUR");
            var dinero2 = new Dinero(100, "EUR");

            // Act & Assert
            dinero1.Should().Be(dinero2);
            (dinero1 == dinero2).Should().BeTrue();
            (dinero1 != dinero2).Should().BeFalse();
            dinero1.GetHashCode().Should().Be(dinero2.GetHashCode());
        }

        [Fact]
        public void ValueObjects_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var dinero1 = new Dinero(100, "EUR");
            var dinero2 = new Dinero(100, "USD");
            var dinero3 = new Dinero(200, "EUR");

            // Act & Assert
            dinero1.Should().NotBe(dinero2);
            dinero1.Should().NotBe(dinero3);
            (dinero1 == dinero2).Should().BeFalse();
            (dinero1 != dinero2).Should().BeTrue();
        }

        [Fact]
        public void ValueObject_ShouldBeEqual_ToItself()
        {
            // Arrange
            var dinero = new Dinero(100, "EUR");

            // Act & Assert
            dinero.Should().Be(dinero);
            dinero.Equals(dinero).Should().BeTrue();
        }

        [Fact]
        public void ValueObject_ShouldNotBeEqual_ToNull()
        {
            // Arrange
            var dinero = new Dinero(100, "EUR");

            // Act & Assert
            dinero.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void ValueObject_ShouldNotBeEqual_ToDifferentType()
        {
            // Arrange
            var dinero = new Dinero(100, "EUR");
            var other = new object();

            // Act & Assert
            dinero.Equals(other).Should().BeFalse();
        }
    }
} 