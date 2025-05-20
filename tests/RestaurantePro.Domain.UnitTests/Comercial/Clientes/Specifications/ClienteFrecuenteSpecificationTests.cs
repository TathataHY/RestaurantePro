namespace RestaurantePro.Domain.UnitTests.Comercial.Clientes.Specifications
{
    public class ClienteFrecuenteSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_ConClienteNull_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();
            
            // Act
            var result = specification.IsSatisfiedBy(null);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConClienteInactivo_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification();
            
            // Crear un cliente y marcarlo como inactivo
            var nombre = ClienteNombre.Crear("Juan", "Pérez");
            var cliente = Cliente.Crear(
                nombre,
                "juan@ejemplo.com",
                "+5491155554444"
            );
            
            // Desactivar cliente utilizando reflexión ya que es un método interno
            typeof(Cliente).GetMethod("Desactivar", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(cliente, null);
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConClienteNuevo_DebeRetornarFalseConAntiguedadMinima()
        {
            // Arrange
            int diasAntiguedadMinima = 30;
            var fechaActual = DateTime.Now;
            
            var specification = new RestaurantePro.Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification(
                diasAntiguedadMinima: diasAntiguedadMinima,
                fechaReferencia: fechaActual
            );
            
            // Crear un cliente "nuevo" (con fecha de creación reciente)
            var nombre = ClienteNombre.Crear("Ana", "García");
            var cliente = Cliente.Crear(
                nombre,
                "ana@ejemplo.com",
                "+5491155556666"
            );
            
            // Ajustar la fecha de creación a 10 días atrás (menos que el mínimo)
            typeof(EntityBase).GetProperty("FechaCreacion")
                .SetValue(cliente, fechaActual.AddDays(-10));
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConClienteConPocasVisitas_DebeRetornarFalse()
        {
            // Arrange
            int visitasMinimas = 5;
            var specification = new RestaurantePro.Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification(
                visitasMinimas: visitasMinimas
            );
            
            // Crear un cliente con tarjeta de fidelización
            var nombre = ClienteNombre.Crear("Carlos", "López");
            var cliente = Cliente.Crear(
                nombre,
                "carlos@ejemplo.com",
                "+5491155557777"
            );
            
            // Crear y asignar tarjeta con pocos puntos (= pocas visitas)
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "TF-12345");
            
            // Asignar puntos que equivalen a menos visitas que el mínimo
            // En la especificación, 100 puntos = 1 visita
            typeof(TarjetaFidelizacion).GetProperty("PuntosActuales")
                .SetValue(tarjeta, (visitasMinimas - 1) * 100);
                
            // Asignar tarjeta al cliente usando TarjetaFidelizacionPrincipalId
            typeof(Cliente).GetProperty("TarjetaFidelizacionPrincipalId")
                .SetValue(cliente, tarjeta.Id);
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConClienteFrecuente_DebeRetornarTrue()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Comercial.Clientes.Specifications.ClienteFrecuenteSpecification(
                visitasMinimas: 3,
                gastoPromedioMinimo: 30.0m,
                diasAntiguedadMinima: 60
            );
            
            // Crear un cliente que cumple todos los criterios
            var nombre = ClienteNombre.Crear("María", "Rodríguez");
            var cliente = Cliente.Crear(
                nombre,
                "maria@ejemplo.com",
                "+5491155558888"
            );
            
            // Establecer fecha de creación antigua (más de 60 días)
            typeof(EntityBase).GetProperty("FechaCreacion")
                .SetValue(cliente, DateTime.Now.AddDays(-90));
            
            // Crear tarjeta con nivel Oro (que según la implementación tiene gasto promedio de 50)
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "TF-67890");
            
            // Establecer nivel Oro
            typeof(TarjetaFidelizacion).GetProperty("NivelFidelizacion")
                .SetValue(tarjeta, NivelFidelizacion.Oro);
                
            // Establecer puntos suficientes (al menos 300 = 3 visitas)
            typeof(TarjetaFidelizacion).GetProperty("PuntosActuales")
                .SetValue(tarjeta, 500);
                
            // Asignar tarjeta al cliente usando TarjetaFidelizacionPrincipalId
            typeof(Cliente).GetProperty("TarjetaFidelizacionPrincipalId")
                .SetValue(cliente, tarjeta.Id);
            
            // Act
            var result = specification.IsSatisfiedBy(cliente);
            
            // Assert
            Assert.True(result);
        }
    }
} 