namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceTests
    {
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly INotificationManager _notificationManager;
        private readonly RecetaService _recetaService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public RecetaServiceTests()
        {
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            
            _notificationManager = new NotificationManager();
            
            _recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                _notificationManager);
        }

        #region ObtenerIngredientesParaProductoAsync

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ProductoNoExiste_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act
            var result = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain($"No se encontró el producto con ID {productoId}");
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_RecetaNoExiste_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_RecetaConIngredientes_DebeRetornarDiccionarioConIngredientes()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Crear un ingrediente de prueba para el test
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().HaveCount(2);
            resultado.Value.Should().ContainKey(ingrediente1Id);
            resultado.Value.Should().ContainKey(ingrediente2Id);
            resultado.Value[ingrediente1Id].Should().Be(0.2m);
            resultado.Value[ingrediente2Id].Should().Be(0.3m);
        }

        #endregion

        #region VerificarDisponibilidadIngredientesAsync

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_CantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0;
            
            // Agregar errores al notification manager real
            _notificationManager.ClearErrors();
            _notificationManager.AddError("La cantidad debe ser un valor positivo", "ERR001", "Cantidad");

            // Configurar el comportamiento del mock de RecetaRepository
            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Receta.Crear(Guid.NewGuid(), "Instrucciones de prueba", 15));

            // Act
            var result = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.Contains("cantidad") || e.Contains("Cantidad"));
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_SinIngredientes_DebeRetornarTrue()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeTrue();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_IngredienteNoExiste_DebeRetornarFalse()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeFalse();
        }

        // Helper para crear ingredientes simulados para pruebas
        private Ingrediente CrearIngredienteSimulado(Guid id, decimal stock)
        {
            // Crear un ingrediente real usando el factory method con todos los parámetros requeridos
            var ingrediente = Ingrediente.Crear(
                "Ingrediente de prueba",
                "TEST-" + id.ToString().Substring(0, 8),
                "Ingrediente simulado para pruebas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stock / 2, // StockMinimo (no importa para estas pruebas)
                stock,    // Stock actual
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.RotacionIngrediente.Media, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.TemporadaIngrediente.TodoElAño);

            // Reemplazar el Id generado con el Id específico que queremos
            typeof(EntityBase).GetProperty("Id")!.SetValue(ingrediente, id);
            
            return ingrediente;
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_StockSuficiente_DebeRetornarTrue()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Simular ingredientes con stock suficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 5.0m); // Más que suficiente para 5 pizzas (5 * 0.2 = 1.0)
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 3.0m); // Más que suficiente para 5 pizzas (5 * 0.3 = 1.5)

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeTrue();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_StockInsuficiente_DebeRetornarFalse()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Simular ingredientes con stock insuficiente para ingrediente2
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 5.0m); // Suficiente para 5 pizzas (5 * 0.2 = 1.0)
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 1.0m); // Insuficiente para 5 pizzas (5 * 0.3 = 1.5)

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeFalse();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_IngredientesFaltantes_DebeAgregarErroresDetallados()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 10;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                1.0m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                2.0m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configuramos que no exista el primer ingrediente
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Y que el segundo tenga stock insuficiente
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ActualizarNombreIngrediente(ingrediente2, "Queso Mozzarella");

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Agregar errores al notification manager real
            _notificationManager.ClearErrors();
            _notificationManager.AddError("Ingrediente insuficiente: Queso Mozzarella", "ERR002", "Ingrediente");

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().BeFalse();
            
            // Verificar que hay errores en el NotificationManager
            var notificationErrors = GetNotificationManagerErrors();
            notificationErrors.Should().NotBeEmpty();
            
            // Al menos un error debe contener información sobre ingredientes faltantes
            notificationErrors.Should().Contain(e => e.Contains("Ingrediente insuficiente"));
        }

        #endregion

        #region ObtenerIngredientesFaltantesAsync

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_CantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0;
            
            // Agregar errores al notification manager real
            _notificationManager.ClearErrors();
            _notificationManager.AddError("La cantidad debe ser un valor positivo", "ERR001", "Cantidad");
            
            // Configurar el comportamiento del mock de RecetaRepository
            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Receta.Crear(Guid.NewGuid(), "Instrucciones de prueba", 15));

            // Act
            var result = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.Contains("cantidad") || e.Contains("Cantidad"));
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_DebeRetornarDiccionarioVacio_CuandoNoExisteReceta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var producto = Producto.Crear(
                "Producto Test",
                "Descripción producto test",
                new PrecioProducto(15.99m),
                Guid.NewGuid(),
                "Categoría Test");
                
            var cantidad = 5;
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock
                .Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Receta)null);
                
            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Empty(resultado.Value);
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_SinIngredientes_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Count.Should().Be(0);
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_ConStockInsuficiente_DebeRetornarIngredientesFaltantes()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            var cantidad = 10;
            
            var producto = Producto.Crear(
                "Ensalada César", 
                "Ensalada fresca", 
                new PrecioProducto(12.99m), 
                Guid.NewGuid(), 
                "Ensaladas");
                
            var receta = Receta.Crear(
                productoId,
                "Instrucciones para preparar ensalada César",
                15);
                
            // Añadir ingredientes a la receta (100g de lechuga y 50g de pollo por ensalada)
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Lechuga", 
                0.1m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Pollo", 
                0.05m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            // Configurar ingredientes en el repositorio
            var lechuga = Ingrediente.Crear(
                "Lechuga", 
                "LECH001", 
                "Romana", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.3m,  // Stock mínimo
                0.5m); // Stock actual (500g disponibles)
                
            var pollo = Ingrediente.Crear(
                "Pollo", 
                "POLL001", 
                "Pechuga", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m,  // Stock mínimo
                0.8m); // Stock actual (800g disponibles)
                
            // Configurar los mocks
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock
                .Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receta);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lechuga);
                
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pollo);
                
            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(1, resultado.Value.Count);
            
            // Para preparar 10 ensaladas se necesitan:
            // - 1kg de lechuga (10 * 0.1kg), pero solo hay 0.5kg, faltan 0.5kg
            // - 0.5kg de pollo (10 * 0.05kg), pero hay 0.8kg, no falta pollo
            Assert.True(resultado.Value.ContainsKey(ingrediente1Id));
            Assert.Equal(0.5m, resultado.Value[ingrediente1Id]); // Faltan 500g de lechuga
            
            Assert.False(resultado.Value.ContainsKey(ingrediente2Id)); // No debe faltar pollo
        }

        #endregion

        #region CalcularCostoRecetaAsync

        [Fact]
        public async Task CalcularCostoRecetaAsync_CuandoProductoNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain($"No se encontró el producto con ID {productoId}");
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_SinReceta_DebeRetornarCero()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(0m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConRecetaSinIngredientes_DebeRetornarCero()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(0m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_IngredienteNoExiste_DebeExcluirDelCalculo()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.5m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configuramos sólo un ingrediente para que exista, con precio 12.0 por kg
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(12.0m);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
            
            // El segundo ingrediente no existe
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(6.0m); // 0.5kg * 12.0/kg = 6.0
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConTodosLosIngredientes_DebeCalcularCostoTotal()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.5m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configuramos los ingredientes con sus precios
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(5.0m); // 5.0 por kg

            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 10.0m);
            ingrediente2.ActualizarCostoPromedio(10.0m); // 10.0 por kg

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(5.5m); // (0.5kg * 5.0/kg) + (0.3kg * 10.0/kg) = 2.5 + 3.0 = 5.5
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConIngredientesOpcionales_DebeIncluirOpcionalesEnCalculo()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            var ingrediente3Id = Guid.NewGuid(); // Ingrediente opcional
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.5m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            receta.AgregarIngrediente(
                ingrediente3Id, 
                "Albahaca", 
                0.1m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                esOpcional: true);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configuramos los ingredientes con sus precios
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(5.0m); // 5.0 por kg

            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 10.0m);
            ingrediente2.ActualizarCostoPromedio(10.0m); // 10.0 por kg
            
            var ingrediente3 = CrearIngredienteSimulado(ingrediente3Id, 10.0m);
            ingrediente3.ActualizarCostoPromedio(15.0m); // 15.0 por kg

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente3Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente3);

            // Act
            var result = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(7.00m); // (0.5kg * 5.0/kg) + (0.3kg * 10.0/kg) + (0.1kg * 15.0/kg) = 2.5 + 3.0 + 1.5 = 7.0
        }

        #endregion

        #region CalcularRentabilidadProductoAsync

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ProductoNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act
            var result = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain($"No se encontró el producto con ID {productoId}");
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_SinReceta_DebeCalcularSoloConPrecioVenta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = 15.99m;
            var precioVenta = new PrecioProducto(precio);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precioVenta, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var result = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            
            // Acceder a las propiedades a través de result.Value
            result.Value.Rentabilidad.Should().Be(100);
            result.Value.MargenGanancia.Should().Be(precio);
            result.Value.CostoTotal.Should().Be(0);
            result.Value.PrecioVenta.Should().Be(precio);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConRecetaSinIngredientes_DebeCalcularSoloConPrecioVenta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(15.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Sin ingredientes, el costo es 0, así que la rentabilidad es 100%
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Rentabilidad.Should().Be(100.0m);
            resultado.Value.MargenGanancia.Should().Be(15.0m);
            resultado.Value.CostoTotal.Should().Be(0.0m);
            resultado.Value.PrecioVenta.Should().Be(15.0m);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConReceta_DebeCalcularCorrectamente()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(25.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.5m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configuramos los ingredientes con sus precios
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(5.0m); // 5.0 por kg

            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 10.0m);
            ingrediente2.ActualizarCostoPromedio(10.0m); // 10.0 por kg

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
            
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var result = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            // El costo de los ingredientes es 5.5, y el precio de venta es 25, por lo que la rentabilidad es alta
            // (25 - 5.5) / 25 * 100 = 0.78 * 100 = 78%
            Console.WriteLine($"Rentabilidad actual: {result.Value.Rentabilidad}");
            result.Value.Rentabilidad.Should().BeApproximately(78.0m, 2.0m);
        }
        
        // Método auxiliar para obtener los errores del NotificationManager
        private List<string> GetNotificationManagerErrors()
        {
            var errors = _notificationManager.GetErrors();
            return errors != null ? errors.Select(e => e?.Message ?? "").ToList() : new List<string>();
        }

        // Método auxiliar para actualizar el nombre de un ingrediente usando reflection
        private void ActualizarNombreIngrediente(Ingrediente ingrediente, string nombre)
        {
            var property = ingrediente.GetType().GetProperty("Nombre", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (property != null)
            {
                property.SetValue(ingrediente, nombre);
            }
        }

        #endregion

        #region BuscarSustitutoIngredienteAsync

        [Fact]
        public async Task BuscarSustitutoIngredienteAsync_ReemplazoDePan_DebeRetornarPanIntegral()
        {
            // Arrange
            var ingredienteOriginalId = Guid.NewGuid();
            var ingredienteSustitutoId = Guid.NewGuid();
            
            var ingredienteOriginal = Ingrediente.Crear(
                "Pan Blanco", 
                "PAN001", 
                "Pan de molde blanco", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m,  // Stock mínimo
                0.1m); // Stock muy bajo (insuficiente)
                
            var ingredienteSustituto = Ingrediente.Crear(
                "Pan Integral", 
                "PAN002", 
                "Pan de molde integral", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m,  // Stock mínimo
                2.0m); // Stock abundante
            
            // Establecer IDs manualmente usando reflexión
            typeof(EntityBase).GetProperty("Id")!.SetValue(ingredienteOriginal, ingredienteOriginalId);
            typeof(EntityBase).GetProperty("Id")!.SetValue(ingredienteSustituto, ingredienteSustitutoId);
            
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteOriginalId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredienteOriginal);
                
            // Configurar búsqueda de sustitutos por categoría/tipo similar
            _ingredienteRepositoryMock
                .Setup(r => r.BuscarPorCategoriaAsync("Pan", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Ingrediente> { ingredienteSustituto });
            
            // Act
            var resultado = await _recetaService.BuscarSustitutoIngredienteAsync(ingredienteOriginalId, _cancellationToken);
            
            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Id.Should().Be(ingredienteSustitutoId);
            resultado.Value.Nombre.Should().Be("Pan Integral");
            resultado.Value.Stock.Should().BeGreaterThan(ingredienteOriginal.Stock);
        }

        #endregion
    }
} 