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
            
            // Act & Assert
            // En lugar de pasar null directamente, verificamos que la expresión retorne false
            // cuando se evalúa con un cliente que es null
            var expr = specification.ToExpression();
            var compiled = expr.Compile();
            
            // No pasamos null directamente sino que comprobamos el comportamiento
            // esperado cuando la expresión evalúa un cliente nulo
            Assert.False(compiled.Invoke(default!));
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
                "+5491155554444",
                DateTime.Now.AddYears(-30)
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
                "+5491155557777",
                DateTime.Now.AddYears(-30)
            );
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSatisfiedBy_ClienteConMuchasVisitas_DebeRetornarTrue()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));
            
            // Simular muchas visitas
            for (int i = 0; i < 15; i++)
            {
                cliente.RegistrarVisita();
            }

            var specification = new Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();

            // Act
            var resultado = specification.IsSatisfiedBy(cliente);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public void IsSatisfiedBy_ClienteConPocasVisitas_DebeRetornarFalse()
        {
            // Arrange
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(nombre, "juan@example.com", "612345678", DateTime.Now.AddYears(-30));
            
            // Simular pocas visitas
            for (int i = 0; i < 3; i++)
            {
                cliente.RegistrarVisita();
            }

            var specification = new Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();

            // Act
            var resultado = specification.IsSatisfiedBy(cliente);

            // Assert
            resultado.Should().BeFalse();
        }
    }
} 
