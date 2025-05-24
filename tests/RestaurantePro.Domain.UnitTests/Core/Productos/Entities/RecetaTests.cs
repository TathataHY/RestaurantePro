namespace RestaurantePro.Domain.UnitTests.Core.Productos.Entities
{
    public class RecetaTests
    {
        private Guid _productoId;

        public RecetaTests()
        {
            _productoId = Guid.NewGuid();
        }

        #region Creación

        [Fact]
        public void Crear_ConDatosValidos_DebeCrearReceta()
        {
            // Arrange
            var preparacion = "Mezclar todos los ingredientes y servir frío.";
            var tiempoPreparacion = 15;

            // Act
            var receta = Receta.Crear(_productoId, preparacion, tiempoPreparacion);

            // Assert
            receta.Should().NotBeNull();
            receta.ProductoId.Should().Be(_productoId);
            receta.Preparacion.Should().Be(preparacion);
            receta.TiempoPreparacionMinutos.Should().Be(tiempoPreparacion);
            receta.Ingredientes.Should().BeEmpty();
        }

        [Fact]
        public void Crear_ConProductoIdVacio_DebeLanzarException()
        {
            // Arrange
            var productoIdVacio = Guid.Empty;
            var preparacion = "Mezclar todos los ingredientes y servir frío.";
            var tiempoPreparacion = 15;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                Receta.Crear(productoIdVacio, preparacion, tiempoPreparacion));
            
            exception.Message.Should().Contain("producto");
            exception.ParamName.Should().Be("productoId");
        }

        [Fact]
        public void Crear_ConPreparacionVacia_DebeLanzarException()
        {
            // Arrange
            var preparacionVacia = "";
            var tiempoPreparacion = 15;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                Receta.Crear(_productoId, preparacionVacia, tiempoPreparacion));
            
            exception.Message.Should().Contain("preparación");
            exception.ParamName.Should().Be("preparacion");
        }

        [Fact]
        public void Crear_ConTiempoPreparacionCero_DebeLanzarException()
        {
            // Arrange
            var preparacion = "Mezclar todos los ingredientes y servir frío.";
            var tiempoPreparacionCero = 0;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                Receta.Crear(_productoId, preparacion, tiempoPreparacionCero));
            
            exception.Message.Should().Contain("tiempo");
            exception.ParamName.Should().Be("tiempoPreparacionMinutos");
        }

        [Fact]
        public void Crear_ConTiempoPreparacionNegativo_DebeLanzarException()
        {
            // Arrange
            var preparacion = "Mezclar todos los ingredientes y servir frío.";
            var tiempoPreparacionNegativo = -5;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                Receta.Crear(_productoId, preparacion, tiempoPreparacionNegativo));
            
            exception.Message.Should().Contain("tiempo");
            exception.ParamName.Should().Be("tiempoPreparacionMinutos");
        }

        #endregion

        #region Agregar Ingredientes

        [Fact]
        public void AgregarIngrediente_ConDatosValidos_DebeAgregarIngrediente()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var esOpcional = false;

            // Act
            receta.AgregarIngrediente(ingredienteId, nombre, cantidad, unidadMedida, esOpcional);

            // Assert
            receta.Ingredientes.Should().HaveCount(1);
            var ingrediente = receta.Ingredientes.First();
            ingrediente.IngredienteId.Should().Be(ingredienteId);
            ingrediente.Nombre.Should().Be(nombre);
            ingrediente.Cantidad.Should().Be(cantidad);
            ingrediente.UnidadMedida.Should().Be(unidadMedida);
            ingrediente.EsOpcional.Should().Be(esOpcional);
        }

        [Fact]
        public void AgregarIngrediente_ConIngredienteExistente_DebeLanzarException()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            
            // Agregamos una primera vez el ingrediente
            receta.AgregarIngrediente(ingredienteId, nombre, cantidad, unidadMedida);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                receta.AgregarIngrediente(ingredienteId, nombre, cantidad, unidadMedida));
        }

        #endregion

        #region Eliminar Ingredientes

        [Fact]
        public void EliminarIngrediente_ConIngredienteExistente_DebeEliminarIngrediente()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            
            receta.AgregarIngrediente(ingredienteId, nombre, cantidad, unidadMedida);

            // Act
            receta.EliminarIngrediente(ingredienteId);

            // Assert
            receta.Ingredientes.Should().BeEmpty();
        }

        [Fact]
        public void EliminarIngrediente_ConIngredienteInexistente_NoDebeHacerNada()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidad = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            
            receta.AgregarIngrediente(ingredienteId1, nombre, cantidad, unidadMedida);

            // Act - Intentamos eliminar un ingrediente que no existe
            receta.EliminarIngrediente(ingredienteId2);

            // Assert - El ingrediente original debe seguir ahí
            receta.Ingredientes.Should().HaveCount(1);
            receta.Ingredientes.First().IngredienteId.Should().Be(ingredienteId1);
        }

        #endregion

        #region Actualizar Ingredientes

        [Fact]
        public void ActualizarCantidadIngrediente_ConIngredienteExistente_DebeActualizarCantidad()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidadInicial = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var nuevaCantidad = 0.5m;
            
            receta.AgregarIngrediente(ingredienteId, nombre, cantidadInicial, unidadMedida);

            // Act
            receta.ActualizarCantidadIngrediente(ingredienteId, nuevaCantidad);

            // Assert
            var ingrediente = receta.Ingredientes.First();
            ingrediente.Cantidad.Should().Be(nuevaCantidad);
        }

        [Fact]
        public void ActualizarCantidadIngrediente_ConCantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidadInicial = 0.2m;
            var unidadMedida = RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo;
            var nuevaCantidadCero = 0m;
            
            receta.AgregarIngrediente(ingredienteId, nombre, cantidadInicial, unidadMedida);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                receta.ActualizarCantidadIngrediente(ingredienteId, nuevaCantidadCero));
        }

        [Fact]
        public void ActualizarCantidadIngrediente_ConIngredienteInexistente_DebeLanzarException()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId = Guid.NewGuid();
            var nuevaCantidad = 0.5m;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                receta.ActualizarCantidadIngrediente(ingredienteId, nuevaCantidad));
        }

        #endregion

        #region Métodos de Negocio

        [Fact]
        public void ObtenerIngredientesRequeridos_ConIngredientes_DebeRetornarDiccionarioConIngredientesNoOpcionales()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);
            var ingredienteId1 = Guid.NewGuid();
            var ingredienteId2 = Guid.NewGuid();
            var ingredienteId3 = Guid.NewGuid();
            
            // Agregar dos ingredientes requeridos y uno opcional
            receta.AgregarIngrediente(ingredienteId1, "Tomate", 0.2m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, false);
            receta.AgregarIngrediente(ingredienteId2, "Queso", 0.3m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, false);
            receta.AgregarIngrediente(ingredienteId3, "Orégano", 0.01m, RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo, true);

            // Act
            var ingredientesRequeridos = receta.ObtenerIngredientesRequeridos();

            // Assert
            ingredientesRequeridos.Should().HaveCount(2);
            ingredientesRequeridos.Should().ContainKey(ingredienteId1);
            ingredientesRequeridos.Should().ContainKey(ingredienteId2);
            ingredientesRequeridos.Should().NotContainKey(ingredienteId3);
            ingredientesRequeridos[ingredienteId1].Should().Be(0.2m);
            ingredientesRequeridos[ingredienteId2].Should().Be(0.3m);
        }

        [Fact]
        public void ObtenerIngredientesRequeridos_SinIngredientes_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var receta = Receta.Crear(_productoId, "Instrucciones de preparación", 15);

            // Act
            var ingredientesRequeridos = receta.ObtenerIngredientesRequeridos();

            // Assert
            ingredientesRequeridos.Should().BeEmpty();
        }

        #endregion
    }
} 