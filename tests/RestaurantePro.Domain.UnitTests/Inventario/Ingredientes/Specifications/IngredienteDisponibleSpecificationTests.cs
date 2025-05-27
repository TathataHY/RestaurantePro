namespace RestaurantePro.Domain.UnitTests.Inventario.Ingredientes.Specifications
{
    public class IngredienteDisponibleSpecificationTests
    {
        [Fact]
        public void IsSatisfiedBy_ConIngredienteNull_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification();
            
            // Act & Assert
            // En lugar de pasar null directamente, verificamos que la expresión retorne false
            // cuando se evalúa con un ingrediente que es null
            var expr = specification.ToExpression();
            var compiled = expr.Compile();
            
            // No pasamos null directamente sino que comprobamos el comportamiento
            // esperado cuando la expresión evalúa un ingrediente nulo
            Assert.False(compiled.Invoke(default!));
        }
        
        [Fact]
        public void IsSatisfiedBy_ConIngredienteEliminado_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification();
            
            var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // stockMinimo
                10.0m // stockInicial
            );
            
            // Marcar como eliminado
            typeof(EntityBase).GetMethod("MarkAsDeleted", BindingFlags.Public | BindingFlags.Instance)
                .Invoke(ingrediente, null);
            
            // Act
            var result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConStockCero_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification();
            
            var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // stockMinimo
                0.0m // stockInicial (cero)
            );
            
            // Act
            var result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConControlCalidadActivado_DebeRetornarFalse()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification(
                verificarControlCalidad: true
            );
            
            var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // stockMinimo
                10.0m // stockInicial
            );
            
            // Simular bloqueo por control de calidad
            typeof(RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente)
                .GetProperty("BloqueadoControlCalidad")
                .SetValue(ingrediente, true);
            
            // Act
            var result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConIngredienteDisponible_DebeRetornarTrue()
        {
            // Arrange
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification();
            
            var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                5.0m, // stockMinimo
                10.0m // stockInicial
            );
            
            // Act
            var result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public void IsSatisfiedBy_ConCantidadMinima_DebeValidarSegunParametro()
        {
            // Arrange
            decimal cantidadMinima = 5.0m;
            var specification = new RestaurantePro.Domain.Inventario.Ingredientes.Specifications.IngredienteDisponibleSpecification(
                cantidadMinima: cantidadMinima
            );
            
            var ingrediente = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Tomate",
                "TOM-001",
                "Tomate para ensaladas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                2.0m, // stockMinimo
                4.0m // stockInicial (menor que cantidadMinima)
            );
            
            // Act
            var result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.False(result);
            
            // Actualizar stock por encima del mínimo
            typeof(RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente)
                .GetProperty("Stock")
                .SetValue(ingrediente, cantidadMinima + 1);
                
            // Act again
            result = specification.IsSatisfiedBy(ingrediente);
            
            // Assert
            Assert.True(result);
        }
    }
} 