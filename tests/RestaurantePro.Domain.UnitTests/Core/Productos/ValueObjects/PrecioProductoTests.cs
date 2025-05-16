
namespace RestaurantePro.Domain.UnitTests.Core.Productos.ValueObjects
{
    public class PrecioProductoTests
    {
        [Fact]
        public void CrearPrecioProducto_ConValorValido_DeberiaCrearCorrectamente()
        {
            // Arrange & Act
            var precio = new PrecioProducto(10.99m);
            
            // Assert
            precio.Valor.Should().Be(10.99m);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void CrearPrecioProducto_ConValorInvalido_DeberiaTirarExcepcion(decimal valorInvalido)
        {
            // Arrange, Act & Assert
            FluentActions.Invoking(() => new PrecioProducto(valorInvalido))
                .Should().Throw<ArgumentException>()
                .WithMessage("*precio debe ser mayor que cero*");
        }
        
        [Fact]
        public void DosPreciosIguales_DeberianSerIguales()
        {
            // Arrange
            var precio1 = new PrecioProducto(10.99m);
            var precio2 = new PrecioProducto(10.99m);
            
            // Act & Assert
            precio1.Should().Be(precio2);
            (precio1 == precio2).Should().BeTrue();
            (precio1 != precio2).Should().BeFalse();
        }
        
        [Fact]
        public void DosPreciosDiferentes_NoDeberianSerIguales()
        {
            // Arrange
            var precio1 = new PrecioProducto(10.99m);
            var precio2 = new PrecioProducto(20.50m);
            
            // Act & Assert
            precio1.Should().NotBe(precio2);
            (precio1 == precio2).Should().BeFalse();
            (precio1 != precio2).Should().BeTrue();
        }
    }
} 