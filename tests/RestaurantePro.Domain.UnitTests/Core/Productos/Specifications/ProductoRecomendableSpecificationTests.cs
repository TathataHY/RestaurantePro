namespace RestaurantePro.Domain.UnitTests.Core.Productos.Specifications
{
    public class ProductoRecomendableSpecificationTests
    {
        #region ToExpression

        [Fact]
        public void ToExpression_ProductoConRequisitosBasicos_DebeRetornarTrue()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            // Establecer popularidad mínima requerida
            producto.ActualizarPopularidad(6);
            
            var spec = new ProductoRecomendableSpecification(popularidadMinima: 5);
            
            // Act
            var expression = spec.ToExpression();
            var compiledExpression = expression.Compile();
            var resultado = compiledExpression(producto);
            
            // Assert
            resultado.Should().BeTrue();
        }
        
        [Fact]
        public void ToExpression_ProductoInactivo_DebeRetornarFalse()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            producto.Desactivar();
            
            var spec = new ProductoRecomendableSpecification(popularidadMinima: 5);
            
            // Act
            var expression = spec.ToExpression();
            var compiledExpression = expression.Compile();
            var resultado = compiledExpression(producto);
            
            // Assert
            resultado.Should().BeFalse();
        }
        
        [Fact]
        public void ToExpression_ProductoConPopularidadBaja_DebeRetornarFalse()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(3); // Popularidad por debajo del mínimo
            
            var spec = new ProductoRecomendableSpecification(popularidadMinima: 5);
            
            // Act
            var expression = spec.ToExpression();
            var compiledExpression = expression.Compile();
            var resultado = compiledExpression(producto);
            
            // Assert
            resultado.Should().BeFalse();
        }

        #endregion
        
        #region IsSatisfiedBy
        
        [Fact]
        public void IsSatisfiedBy_SinRecetaService_DebeVerificarSoloCriteriosBasicos()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            
            var spec = new ProductoRecomendableSpecification(
                rentabilidadMinima: 30.0m,
                popularidadMinima: 5,
                verificarDisponibilidadIngredientes: false);
            
            // Act
            var resultado = spec.IsSatisfiedBy(producto);
            
            // Assert
            resultado.Should().BeTrue();
        }
        
        [Fact]
        public void IsSatisfiedBy_ConRecetaService_VerificaRentabilidad()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            
            var recetaServiceMock = new Mock<IRecetaService>();
            
            // Configurar la respuesta del servicio con rentabilidad por debajo del mínimo
            recetaServiceMock.Setup(r => r.CalcularRentabilidadProductoAsync(
                    producto.Id, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(RestaurantePro.Domain.Core.Productos.ValueObjects.RentabilidadProducto.Calcular(8.0m, 10.99m));
                
            var spec = new ProductoRecomendableSpecification(
                rentabilidadMinima: 50.0m, // La rentabilidad simulada es 27.2%
                popularidadMinima: 5,
                verificarDisponibilidadIngredientes: false,
                recetaService: recetaServiceMock.Object);
            
            // Act
            var resultado = spec.IsSatisfiedBy(producto);
            
            // Assert
            resultado.Should().BeFalse();
        }
        
        [Fact]
        public void IsSatisfiedBy_ConRentabilidadSuficiente_DebeRetornarTrue()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            
            var recetaServiceMock = new Mock<IRecetaService>();
            
            // Configurar la respuesta del servicio con rentabilidad alta
            recetaServiceMock.Setup(r => r.CalcularRentabilidadProductoAsync(
                    producto.Id, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(RestaurantePro.Domain.Core.Productos.ValueObjects.RentabilidadProducto.Calcular(2.0m, 10.99m));
                
            var spec = new ProductoRecomendableSpecification(
                rentabilidadMinima: 50.0m, // La rentabilidad simulada es 81.8%
                popularidadMinima: 5,
                verificarDisponibilidadIngredientes: false,
                recetaService: recetaServiceMock.Object);
            
            // Act
            var resultado = spec.IsSatisfiedBy(producto);
            
            // Assert
            resultado.Should().BeTrue();
        }
        
        [Fact]
        public void IsSatisfiedBy_VerificandoDisponibilidadSinIngredientes_DebeRetornarTrue()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            
            var recetaServiceMock = new Mock<IRecetaService>();
            
            // Configurar la respuesta del servicio con rentabilidad alta y disponibilidad
            recetaServiceMock.Setup(r => r.CalcularRentabilidadProductoAsync(
                    producto.Id, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(RestaurantePro.Domain.Core.Productos.ValueObjects.RentabilidadProducto.Calcular(2.0m, 10.99m));
                
            recetaServiceMock.Setup(r => r.VerificarDisponibilidadIngredientesAsync(
                    producto.Id,
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            var spec = new ProductoRecomendableSpecification(
                rentabilidadMinima: 50.0m,
                popularidadMinima: 5,
                verificarDisponibilidadIngredientes: true,
                recetaService: recetaServiceMock.Object);
            
            // Act
            var resultado = spec.IsSatisfiedBy(producto);
            
            // Assert
            resultado.Should().BeTrue();
        }
        
        [Fact]
        public void IsSatisfiedBy_VerificandoDisponibilidadSinIngredientesDisponibles_DebeRetornarFalse()
        {
            // Arrange
            var precio = new PrecioProducto(10.99m);
            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
            
            producto.ActualizarPopularidad(6);
            
            var recetaServiceMock = new Mock<IRecetaService>();
            
            // Configurar la respuesta del servicio con rentabilidad alta pero sin disponibilidad
            recetaServiceMock.Setup(r => r.CalcularRentabilidadProductoAsync(
                    producto.Id, 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(RestaurantePro.Domain.Core.Productos.ValueObjects.RentabilidadProducto.Calcular(2.0m, 10.99m));
                
            recetaServiceMock.Setup(r => r.VerificarDisponibilidadIngredientesAsync(
                    producto.Id,
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
                
            var spec = new ProductoRecomendableSpecification(
                rentabilidadMinima: 50.0m,
                popularidadMinima: 5,
                verificarDisponibilidadIngredientes: true,
                recetaService: recetaServiceMock.Object);
            
            // Act
            var resultado = spec.IsSatisfiedBy(producto);
            
            // Assert
            resultado.Should().BeFalse();
        }
        
        #endregion
    }
} 