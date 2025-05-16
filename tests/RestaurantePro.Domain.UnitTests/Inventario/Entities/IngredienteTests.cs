// IngredienteTests

using RestaurantePro.Domain.Inventario.Enums;

namespace RestaurantePro.Domain.UnitTests.Inventario.Entities
{
    public class IngredienteTests
    {
        [Fact]
        public void CrearIngrediente_ConDatosValidos_DebeCrearCorrectamente()
        {
            // Arrange
            var nombre = "Tomate";
            var unidadMedida = UnidadMedida.Kilogramo;
            var stockMinimo = 5.0m;
            
            // Act
            var ingrediente = Domain.Inventario.Entities.Ingrediente.Crear(nombre, unidadMedida, stockMinimo);
            
            // Assert
            ingrediente.Should().NotBeNull();
            ingrediente.Nombre.Should().Be(nombre);
            ingrediente.UnidadMedida.Should().Be(unidadMedida);
            ingrediente.StockMinimo.Should().Be(stockMinimo);
            ingrediente.Stock.Should().Be(0);
            ingrediente.EstaActivo.Should().BeTrue();
        }
        
        [Fact]
        public void CrearIngrediente_ConNombreVacio_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "";
            var unidadMedida = UnidadMedida.Kilogramo;
            var stockMinimo = 5.0m;
            
            // Act & Assert
            var action = () => Domain.Inventario.Entities.Ingrediente.Crear(nombre, unidadMedida, stockMinimo);
            action.Should().Throw<ArgumentException>().WithMessage("*nombre*");
        }
        
        [Fact]
        public void CrearIngrediente_ConStockMinimoNegativo_DebeLanzarExcepcion()
        {
            // Arrange
            var nombre = "Tomate";
            var unidadMedida = UnidadMedida.Kilogramo;
            var stockMinimo = -1.0m;
            
            // Act & Assert
            var action = () => Domain.Inventario.Entities.Ingrediente.Crear(nombre, unidadMedida, stockMinimo);
            action.Should().Throw<ArgumentException>().WithMessage("*stock mu00ednimo*");
        }
    }
}
