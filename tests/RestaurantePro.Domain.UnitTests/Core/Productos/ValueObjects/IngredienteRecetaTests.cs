namespace RestaurantePro.Domain.UnitTests.Core.Productos.ValueObjects
{
    public class IngredienteRecetaTests
    {
        #region Creación

        [Fact]
        public void Crear_ConDatosValidos_DebeCrearIngredienteReceta()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act
            var ingredienteReceta = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Assert
            ingredienteReceta.Should().NotBeNull();
            ingredienteReceta.IngredienteId.Should().Be(ingredienteId);
            ingredienteReceta.Nombre.Should().Be(nombre);
            ingredienteReceta.Cantidad.Should().Be(cantidad);
            ingredienteReceta.UnidadMedida.Should().Be(unidadMedida);
            ingredienteReceta.EsOpcional.Should().Be(esOpcional);
        }

        [Fact]
        public void Crear_ConIngredienteIdVacio_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.Empty;
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                IngredienteReceta.Crear(
                    ingredienteId,
                    nombre,
                    cantidad,
                    unidadMedida,
                    esOpcional));
            
            exception.Message.Should().Contain("ingrediente");
            exception.ParamName.Should().Be("ingredienteId");
        }

        [Fact]
        public void Crear_ConNombreVacio_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                IngredienteReceta.Crear(
                    ingredienteId,
                    nombre,
                    cantidad,
                    unidadMedida,
                    esOpcional));
            
            exception.Message.Should().Contain("nombre");
            exception.ParamName.Should().Be("nombre");
        }

        [Fact]
        public void Crear_ConCantidadCero_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                IngredienteReceta.Crear(
                    ingredienteId,
                    nombre,
                    cantidad,
                    unidadMedida,
                    esOpcional));
            
            exception.Message.Should().Contain("cantidad");
            exception.ParamName.Should().Be("cantidad");
        }

        [Fact]
        public void Crear_ConCantidadNegativa_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = -0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                IngredienteReceta.Crear(
                    ingredienteId,
                    nombre,
                    cantidad,
                    unidadMedida,
                    esOpcional));
            
            exception.Message.Should().Contain("cantidad");
            exception.ParamName.Should().Be("cantidad");
        }

        #endregion

        #region Modificación

        [Fact]
        public void ConCantidad_ConCantidadValida_DebeCrearNuevaInstanciaConCantidadActualizada()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;
            var nuevaCantidad = 0.5m;

            var ingredienteReceta = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act
            var nuevoIngredienteReceta = ingredienteReceta.ConCantidad(nuevaCantidad);

            // Assert
            nuevoIngredienteReceta.Should().NotBeNull();
            nuevoIngredienteReceta.Should().NotBeSameAs(ingredienteReceta);
            nuevoIngredienteReceta.IngredienteId.Should().Be(ingredienteId);
            nuevoIngredienteReceta.Nombre.Should().Be(nombre);
            nuevoIngredienteReceta.Cantidad.Should().Be(nuevaCantidad);
            nuevoIngredienteReceta.UnidadMedida.Should().Be(unidadMedida);
            nuevoIngredienteReceta.EsOpcional.Should().Be(esOpcional);
        }

        [Fact]
        public void ConCantidad_ConCantidadCero_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;
            var nuevaCantidad = 0m;

            var ingredienteReceta = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                ingredienteReceta.ConCantidad(nuevaCantidad));
        }

        [Fact]
        public void ConCantidad_ConCantidadNegativa_DebeLanzarException()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;
            var nuevaCantidad = -0.5m;

            var ingredienteReceta = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                ingredienteReceta.ConCantidad(nuevaCantidad));
        }

        [Fact]
        public void ConEsOpcional_DebeCrearNuevaInstanciaConEsOpcionalActualizado()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;
            var nuevoEsOpcional = true;

            var ingredienteReceta = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act
            var nuevoIngredienteReceta = ingredienteReceta.ConEsOpcional(nuevoEsOpcional);

            // Assert
            nuevoIngredienteReceta.Should().NotBeNull();
            nuevoIngredienteReceta.Should().NotBeSameAs(ingredienteReceta);
            nuevoIngredienteReceta.IngredienteId.Should().Be(ingredienteId);
            nuevoIngredienteReceta.Nombre.Should().Be(nombre);
            nuevoIngredienteReceta.Cantidad.Should().Be(cantidad);
            nuevoIngredienteReceta.UnidadMedida.Should().Be(unidadMedida);
            nuevoIngredienteReceta.EsOpcional.Should().Be(nuevoEsOpcional);
        }

        #endregion

        #region Comparación de ValueObject

        [Fact]
        public void EqualsOperator_MismosValores_DebeRetornarTrue()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            var ingredienteReceta1 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            var ingredienteReceta2 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act & Assert
            (ingredienteReceta1 == ingredienteReceta2).Should().BeTrue();
        }

        [Fact]
        public void NotEqualsOperator_DiferentesValores_DebeRetornarTrue()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            var ingredienteReceta1 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            var ingredienteReceta2 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                0.3m, // Cantidad diferente
                unidadMedida,
                esOpcional);

            // Act & Assert
            (ingredienteReceta1 != ingredienteReceta2).Should().BeTrue();
        }

        [Fact]
        public void Equals_MismosValores_DebeRetornarTrue()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            var ingredienteReceta1 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            var ingredienteReceta2 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act & Assert
            ingredienteReceta1.Equals(ingredienteReceta2).Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_MismosValores_DebeRetornarMismoHashCode()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            var ingredienteReceta1 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            var ingredienteReceta2 = IngredienteReceta.Crear(
                ingredienteId,
                nombre,
                cantidad,
                unidadMedida,
                esOpcional);

            // Act & Assert
            ingredienteReceta1.GetHashCode().Should().Be(ingredienteReceta2.GetHashCode());
        }

        #endregion
    }
} 