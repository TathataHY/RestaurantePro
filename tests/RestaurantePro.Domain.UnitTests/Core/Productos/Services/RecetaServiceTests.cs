namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceTests
    {
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly RecetaService _recetaService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public RecetaServiceTests()
        {
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();

            _recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object);
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
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
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

            // Act
            var resultado = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.Should().ContainKey(ingrediente1Id);
            resultado.Should().ContainKey(ingrediente2Id);
            resultado[ingrediente1Id].Should().Be(0.2m);
            resultado[ingrediente2Id].Should().Be(0.3m);
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
            resultado.Should().BeTrue();
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
            resultado.Should().BeFalse();
        }

        // Helper para crear ingredientes simulados para pruebas
        private Ingrediente CrearIngredienteSimulado(Guid id, decimal stock)
        {
            // Crear un ingrediente real usando el factory method
            var ingrediente = Ingrediente.Crear(
                "Ingrediente de prueba",
                "TEST-" + id.ToString().Substring(0, 8),
                "Ingrediente simulado para pruebas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stock / 2, // StockMinimo (no importa para estas pruebas)
                stock);    // Stock actual

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

            // Crear un ingrediente simulado con stock suficiente
            var ingrediente = CrearIngredienteSimulado(ingrediente1Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_StockInsuficiente_DebeRetornarFalse()
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

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente simulado con stock insuficiente
            var ingrediente = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeFalse();
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
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_IngredientesFaltantes_DebeRetornarIngredientesFaltantes()
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

            // Crear ingredientes simulados
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.3 = 1.5kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.Should().ContainKey(ingrediente1Id);
            resultado[ingrediente1Id].Should().Be(0.5m); // Faltante: 1kg - 0.5kg = 0.5kg
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_SinIngredientesFaltantes_DebeRetornarDiccionarioVacio()
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

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente simulado con stock suficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
        }

        #endregion
    }
} 