using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;

namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Specifications
{
    /// <summary>
    /// Pruebas para la especificación ClienteFrecuenteSpecification
    /// </summary>
    public class ClienteFrecuenteSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_ConClienteNull_DebeRetornarFalse()
        {
            // Arrange
            var specification = new Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();
            
            // Act
            var result = specification.IsSatisfiedBy(null);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConClienteInactivo_DebeRetornarFalse()
        {
            // Arrange
            var specification = new Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();
            
            // Crear un cliente y marcarlo como inactivo
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(
                nombre,
                "juan@ejemplo.com",
                "+5491155554444"
            );
            
            // Desactivar cliente mediante método público
            cliente.Desactivar();
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConPocosONingunPunto_DebeRetornarFalse()
        {
            // Arrange - Cliente sin puntos significa pocas visitas
            var specification = new Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification(visitasMinimas: 3);
            
            // Crear un cliente - tendrá 0 puntos y 0 visitas inicialmente
            var nombre = ClienteNombre.Crear("Carlos", "López");
            var cliente = Cliente.Crear(
                nombre,
                "carlos@ejemplo.com",
                "+5491155557777"
            );
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }
    }
} 