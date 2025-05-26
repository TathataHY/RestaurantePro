namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceTests
    {
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly RecetaService _recetaService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public RecetaServiceTests()
        {
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _notificationManagerMock = new Mock<INotificationManager>();

            _recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                _notificationManagerMock.Object);
        }

        #region ObtenerIngredientesParaProductoAsync

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ProductoNoExiste_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken));
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

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken));
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
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);
            var notificationManager = new NotificationManager();

            var recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                notificationManager);

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
                "Queso", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente con stock insuficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)
            ActualizarNombreIngrediente(ingrediente1, "Tomate");

            // El segundo ingrediente no existe en la base de datos

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var resultado = await recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue(); // Retorna True pero con errores en NotificationManager
            resultado.Value.Should().BeFalse(); // El valor indica que no hay disponibilidad
            
            notificationManager.HasErrors.Should().BeTrue();
            notificationManager.GetErrors().Should().HaveCountGreaterThan(1);
            
            // Verificar que hay un error específico para cada ingrediente faltante
            var errores = notificationManager.GetErrors().ToList();
            errores.Should().Contain(e => e.PropertyName == $"Ingrediente_{ingrediente1Id}");
            errores.Should().Contain(e => e.PropertyName == $"Ingrediente_{ingrediente2Id}");
            
            // Verificar que los mensajes contienen información detallada
            var mensajeIngrediente1 = errores.FirstOrDefault(e => e.PropertyName == $"Ingrediente_{ingrediente1Id}")?.Message;
            mensajeIngrediente1.Should().Contain("Tomate");
            mensajeIngrediente1.Should().Contain("0.5"); // Stock disponible
            
            var mensajeIngrediente2 = errores.FirstOrDefault(e => e.PropertyName == $"Ingrediente_{ingrediente2Id}")?.Message;
            mensajeIngrediente2.Should().Contain("Ingrediente no encontrado");
        }

        #endregion

        #region ObtenerIngredientesFaltantesAsync

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_CantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken));
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
        public async Task ObtenerIngredientesFaltantesAsync_IngredientesFaltantes_DebeRetornarIngredientesFaltantes()
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
            var ingrediente3Id = Guid.NewGuid();
            
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
                
            receta.AgregarIngrediente(
                ingrediente3Id, 
                "Albahaca", 
                0.05m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Simular ingredientes con stocks variados
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 5.0m); // Más que suficiente para 5 pizzas (5 * 0.2 = 1.0)
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 1.0m); // Insuficiente para 5 pizzas (5 * 0.3 = 1.5) -> Falta 0.5
            var ingrediente3 = CrearIngredienteSimulado(ingrediente3Id, 0.1m);  // Insuficiente para 5 pizzas (5 * 0.05 = 0.25) -> Falta 0.15

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente3Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente3);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().HaveCount(2);
            resultado.Value.Should().ContainKey(ingrediente2Id);
            resultado.Value.Should().ContainKey(ingrediente3Id);
            resultado.Value[ingrediente2Id].Should().BeApproximately(0.5m, 0.001m);
            resultado.Value[ingrediente3Id].Should().BeApproximately(0.15m, 0.001m);
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_SinIngredientesFaltantes_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                new NotificationManager());

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
            var resultado = await recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().HaveCount(0);
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_ConIngredientesSinStock_DebeRetornarDiccionarioConFaltantes()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                new NotificationManager());

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);

            // Añadir ingredientes directamente a la receta
            receta.AgregarIngrediente(
                ingrediente1Id,
                "Queso Mozzarella",
                2.0m,
                UnidadMedida.Kilogramo);
                
            receta.AgregarIngrediente(
                ingrediente2Id,
                "Salsa de Tomate",
                1.5m,
                UnidadMedida.Litro);

            // Crear ingredientes con stock insuficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 5.0m); // Stock insuficiente para cantidadPersonas * cantidadReceta = 5 * 2 = 10
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 0.0m); // Sin stock (necesita 5 * 1.5 = 7.5)

            // Configurar mocks
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeNull();
            resultado.Value.Should().HaveCount(2);
            resultado.Value.Should().ContainKey(ingrediente1Id);
            resultado.Value.Should().ContainKey(ingrediente2Id);
            resultado.Value[ingrediente1Id].Should().BeApproximately(5.0m, 0.01m); // Faltante: necesita 10, tiene 5
            resultado.Value[ingrediente2Id].Should().BeApproximately(7.5m, 0.01m); // Faltante: necesita 7.5, tiene 0
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

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken));
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
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().Be(0m);
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
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().Be(0m);
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

            // Configurar ObtenerRecetaIngredientesAsync para que devuelva los ingredientes de la receta
            var ingredientesReceta = new List<RestaurantePro.Domain.Core.Productos.ValueObjects.IngredienteReceta>();
            foreach (var ingrediente in receta.Ingredientes)
            {
                ingredientesReceta.Add(ingrediente);
            }
            
            _recetaRepositoryMock.Setup(r => r.ObtenerRecetaIngredientesAsync(receta.Id, _cancellationToken))
                .ReturnsAsync(ingredientesReceta);

            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(20m); // $20 por kilogramo

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Solo debe contar el costo del queso: 0.3kg * 20.0 = 6.0
            resultado.Should().Be(6.0m);
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

            // Configurar ObtenerRecetaIngredientesAsync para que devuelva los ingredientes de la receta
            var ingredientesReceta = new List<RestaurantePro.Domain.Core.Productos.ValueObjects.IngredienteReceta>();
            foreach (var ingrediente in receta.Ingredientes)
            {
                ingredientesReceta.Add(ingrediente);
            }
            
            _recetaRepositoryMock.Setup(r => r.ObtenerRecetaIngredientesAsync(receta.Id, _cancellationToken))
                .ReturnsAsync(ingredientesReceta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(20m); // $20 por kilogramo
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(5m);  // $5 por pieza

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Tomate: 0.2kg * 20.0 = 4.0
            // Queso: 0.3kg * 5.0 = 1.5
            // Total: 5.5
            resultado.Should().Be(5.5m);
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
            var ingrediente3Id = Guid.NewGuid();
            
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
                
            receta.AgregarIngrediente(
                ingrediente3Id, 
                "Aceitunas", 
                0.05m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                true); // Ingrediente opcional

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Configurar ObtenerRecetaIngredientesAsync para que devuelva los ingredientes de la receta
            var ingredientesReceta = new List<RestaurantePro.Domain.Core.Productos.ValueObjects.IngredienteReceta>();
            foreach (var ingrediente in receta.Ingredientes)
            {
                ingredientesReceta.Add(ingrediente);
            }
            
            _recetaRepositoryMock.Setup(r => r.ObtenerRecetaIngredientesAsync(receta.Id, _cancellationToken))
                .ReturnsAsync(ingredientesReceta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(20m); // $20 por kilogramo
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(5m);  // $5 por pieza
            
            var ingrediente3 = CrearIngredienteSimulado(ingrediente3Id, 2.0m);
            ingrediente3.ActualizarCostoPromedio(30m); // $30 por litro

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente3Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente3);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Tomate: 0.2kg * 20.0 = 4.0
            // Queso: 0.3kg * 5.0 = 1.5
            // Aceitunas: 0.05kg * 30.0 = 1.5
            // Total: 7.0
            resultado.Should().Be(7.0m);
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

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken));
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
            var precioVenta = 20.99m;
            
            // Costo total esperado: 9.25
            // Margen: 20.99 - 9.25 = 11.74
            // Rentabilidad: 11.74 / 20.99 = 0.5593 (56%)
            
            // Crear producto
            var precio = new PrecioProducto(precioVenta);
            var producto = Producto.Crear(
                "Pizza Especial", 
                "Pizza con varios ingredientes", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");
                
            // Crear receta con ingredientes
            var receta = Receta.Crear(productoId, "Instrucciones para pizza especial", 45);
            
            // Ids para ingredientes
            var tomateSalsaId = Guid.NewGuid();
            var quesoMozzarellaId = Guid.NewGuid();
            var jamonId = Guid.NewGuid();
            var champignonesId = Guid.NewGuid();
            
            // Agregar ingredientes a la receta
            receta.AgregarIngrediente(tomateSalsaId, "Salsa de Tomate", 0.25m, UnidadMedida.Kilogramo);
            receta.AgregarIngrediente(quesoMozzarellaId, "Queso Mozzarella", 0.35m, UnidadMedida.Kilogramo);
            receta.AgregarIngrediente(jamonId, "Jamón", 0.2m, UnidadMedida.Kilogramo);
            receta.AgregarIngrediente(champignonesId, "Champiñones", 0.15m, UnidadMedida.Kilogramo);
            
            // Configurar repositorios
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(receta);
                
            // Crear ingredientes con precios
            var tomateSalsa = Ingrediente.Crear(
                "Salsa de Tomate", 
                "TSA-001", 
                "Salsa de tomate para pizzas", 
                UnidadMedida.Kilogramo,
                5.0m, // StockMinimo
                10.0m); // Stock actual
            // Establecer el costo: 0.25kg * 10/kg = 2.5
            SetPrivateProperty(tomateSalsa, "CostoPromedio", 10m);
            
            var quesoMozzarella = Ingrediente.Crear(
                "Queso Mozzarella", 
                "QMZ-001", 
                "Queso mozzarella para pizzas", 
                UnidadMedida.Kilogramo,
                2.0m, // StockMinimo
                5.0m); // Stock actual
            // Establecer el costo: 0.35kg * 15/kg = 5.25
            SetPrivateProperty(quesoMozzarella, "CostoPromedio", 15m);
            
            var jamon = Ingrediente.Crear(
                "Jamón", 
                "JAM-001", 
                "Jamón para pizzas", 
                UnidadMedida.Kilogramo,
                1.0m, // StockMinimo
                3.0m); // Stock actual
            // Establecer el costo: 0.2kg * 5/kg = 1.0
            SetPrivateProperty(jamon, "CostoPromedio", 5m);
            
            var champignones = Ingrediente.Crear(
                "Champiñones", 
                "CHA-001", 
                "Champiñones para pizzas", 
                UnidadMedida.Kilogramo,
                1.0m, // StockMinimo
                2.0m); // Stock actual
            // Establecer el costo: 0.15kg * 3.33/kg = 0.5
            SetPrivateProperty(champignones, "CostoPromedio", 3.33m);
            
            // Configurar repositorio de ingredientes
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(tomateSalsaId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tomateSalsa);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(quesoMozzarellaId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(quesoMozzarella);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(jamonId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jamon);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(champignonesId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(champignones);
                
            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.CostoTotal.Should().BeApproximately(9.25m, 0.01m);
            resultado.Value.PrecioVenta.Should().Be(precioVenta);
            resultado.Value.MargenGanancia.Should().BeApproximately(11.74m, 0.01m);
            resultado.Value.Rentabilidad.Should().BeApproximately(0.56m, 0.01m);
        }
        
        // Método auxiliar para establecer propiedades privadas con reflection
        private void SetPrivateProperty<T>(object obj, string propertyName, T value)
        {
            var property = obj.GetType().GetProperty(propertyName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (property != null)
            {
                property.SetValue(obj, value);
            }
        }

        // Método auxiliar para actualizar el nombre de un ingrediente usando reflection
        private void ActualizarNombreIngrediente(Ingrediente ingrediente, string nombre)
        {
            SetPrivateProperty(ingrediente, "Nombre", nombre);
        }

        #endregion

        #region BuscarSustitutoIngredienteAsync

        [Fact]
        public async Task BuscarSustitutoIngredienteAsync_ReemplazoDePan_DebeRetornarPanIntegral()
        {
            // Implementar esta prueba cuando se agregue la función
        }

        #endregion
    }
} 