namespace RestaurantePro.Domain.UnitTests.Core.Services
{
    /// <summary>
    /// Pruebas unitarias para CoreOperacionesIntegrationService
    /// </summary>
    public class CoreOperacionesIntegrationServiceTests
    {
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoRepository> _productoRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<RestaurantePro.Domain.Core.Productos.Services.IRecetaService> _recetaServiceMock;
        private readonly Mock<RestaurantePro.Domain.Core.Base.Services.IDateTimeService> _dateTimeServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly RestaurantePro.Domain.Core.Services.CoreOperacionesIntegrationService _sut;
        
        public CoreOperacionesIntegrationServiceTests()
        {
            _productoRepositoryMock = new Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoRepository>();
            _recetaRepositoryMock = new Mock<RestaurantePro.Domain.Core.Productos.Interfaces.IRecetaRepository>();
            _recetaServiceMock = new Mock<RestaurantePro.Domain.Core.Productos.Services.IRecetaService>();
            _dateTimeServiceMock = new Mock<RestaurantePro.Domain.Core.Base.Services.IDateTimeService>();
            _notificationManager = new NotificationManager();
            
            _sut = new RestaurantePro.Domain.Core.Services.CoreOperacionesIntegrationService(
                _productoRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _recetaServiceMock.Object,
                _notificationManager,
                _dateTimeServiceMock.Object);
        }
        
        #region VerificarDisponibilidadProductosAsync Tests
        
        [Fact]
        public async Task VerificarDisponibilidadProductosAsync_ConProductosDisponibles_DebeRetornarTodosDisponibles()
        {
            // Arrange
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 1 }
            };
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            var producto2 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 2", 
                "Descripción 2", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(15.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto2);
                
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
                
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId2, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            
            // Act
            var result = await _sut.VerificarDisponibilidadProductosAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TodosDisponibles.Should().BeTrue();
            result.Value.ProductosNoDisponibles.Should().BeEmpty();
            result.Value.IngredientesFaltantes.Should().BeEmpty();
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId1, 2, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId2, 1, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task VerificarDisponibilidadProductosAsync_ConProductoNoEncontrado_DebeRetornarNoDisponible()
        {
            // Arrange
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 1 }
            };
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RestaurantePro.Domain.Core.Productos.Entities.Producto)null);
                
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            
            // Act
            var result = await _sut.VerificarDisponibilidadProductosAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TodosDisponibles.Should().BeFalse();
            result.Value.ProductosNoDisponibles.Should().ContainKey(productoId2);
            result.Value.ProductosNoDisponibles[productoId2].Should().Be("Producto no encontrado");
            result.Value.IngredientesFaltantes.Should().BeEmpty();
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId1, 2, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId2, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task VerificarDisponibilidadProductosAsync_ConProductoInactivo_DebeRetornarNoDisponible()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId, 2 }
            };
            
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            producto.Desactivar(); // Desactivar el producto
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
            
            // Act
            var result = await _sut.VerificarDisponibilidadProductosAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TodosDisponibles.Should().BeFalse();
            result.Value.ProductosNoDisponibles.Should().ContainKey(productoId);
            result.Value.ProductosNoDisponibles[productoId].Should().Be("Producto no disponible");
            result.Value.IngredientesFaltantes.Should().BeEmpty();
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task VerificarDisponibilidadProductosAsync_ConIngredientesInsuficientes_DebeRetornarNoDisponible()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId, 2 }
            };
            
            var producto = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(false));
                
            var ingredienteId = Guid.NewGuid();
            var ingredientesFaltantes = new Dictionary<Guid, decimal>
            {
                { ingredienteId, 0.5m }
            };
                
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesFaltantesAsync(
                    productoId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredientesFaltantes));
            
            // Act
            var result = await _sut.VerificarDisponibilidadProductosAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.TodosDisponibles.Should().BeFalse();
            result.Value.ProductosNoDisponibles.Should().ContainKey(productoId);
            result.Value.ProductosNoDisponibles[productoId].Should().Be("Ingredientes insuficientes");
            result.Value.IngredientesFaltantes.Should().NotBeEmpty();
            result.Value.IngredientesFaltantes.Should().ContainKey(ingredienteId);
            result.Value.IngredientesFaltantes[ingredienteId].Should().Be(0.5m);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId, 2, It.IsAny<CancellationToken>()), Times.Once);
            _recetaServiceMock.Verify(s => s.ObtenerIngredientesFaltantesAsync(productoId, 2, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
        
        #region CalcularPreciosTotalesAsync Tests
        
        [Fact]
        public async Task CalcularPreciosTotalesAsync_ConProductosValidos_DebeCalcularCorrectamente()
        {
            // Arrange
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 3 }
            };
            
            var precio1 = 10.99m;
            var precio2 = 15.99m;
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(precio1), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            var producto2 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 2", 
                "Descripción 2", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(precio2), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto2);
            
            // Act
            var result = await _sut.CalcularPreciosTotalesAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            
            var expected1 = precio1 * 2;
            var expected2 = precio2 * 3;
            var expectedTotal = expected1 + expected2;
            
            result.Value.PrecioTotal.Should().Be(expectedTotal);
            result.Value.PreciosUnitarios.Should().ContainKey(productoId1);
            result.Value.PreciosUnitarios.Should().ContainKey(productoId2);
            result.Value.PreciosUnitarios[productoId1].Should().Be(precio1);
            result.Value.PreciosUnitarios[productoId2].Should().Be(precio2);
            result.Value.PreciosPorProducto.Should().ContainKey(productoId1);
            result.Value.PreciosPorProducto.Should().ContainKey(productoId2);
            result.Value.PreciosPorProducto[productoId1].Should().Be(expected1);
            result.Value.PreciosPorProducto[productoId2].Should().Be(expected2);
            result.Value.ProductosNoEncontrados.Should().BeEmpty();
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CalcularPreciosTotalesAsync_ConProductoNoEncontrado_DebeRetornarProductoNoEncontrado()
        {
            // Arrange
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 3 }
            };
            
            var precio1 = 10.99m;
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(precio1), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RestaurantePro.Domain.Core.Productos.Entities.Producto)null);
            
            // Act
            var result = await _sut.CalcularPreciosTotalesAsync(productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().NotBeNull();
            
            var expected1 = precio1 * 2;
            
            result.Value.PrecioTotal.Should().Be(expected1);
            result.Value.PreciosUnitarios.Should().ContainKey(productoId1);
            result.Value.PreciosUnitarios.Should().NotContainKey(productoId2);
            result.Value.PreciosPorProducto.Should().ContainKey(productoId1);
            result.Value.PreciosPorProducto.Should().NotContainKey(productoId2);
            result.Value.ProductosNoEncontrados.Should().ContainSingle();
            result.Value.ProductosNoEncontrados.Should().Contain(productoId2);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
        
        #region ProcesarComandaFinalizadaAsync Tests
        
        [Fact]
        public async Task ProcesarComandaFinalizadaAsync_ConProductosValidos_DebeRetornarExito()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 3 }
            };
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            var producto2 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 2", 
                "Descripción 2", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(15.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto2);
            
            // Act
            var result = await _sut.ProcesarComandaFinalizadaAsync(comandaId, productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ProcesarComandaFinalizadaAsync_ConProductoNoEncontrado_DebeRetornarError()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var productoId1 = Guid.NewGuid();
            var productoId2 = Guid.NewGuid();
            
            var productosIdCantidad = new Dictionary<Guid, int>
            {
                { productoId1, 2 },
                { productoId2, 3 }
            };
            
            var producto1 = RestaurantePro.Domain.Core.Productos.Entities.Producto.Crear(
                "Producto 1", 
                "Descripción 1", 
                new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto1);
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RestaurantePro.Domain.Core.Productos.Entities.Producto)null);
            
            // Act
            var result = await _sut.ProcesarComandaFinalizadaAsync(comandaId, productosIdCantidad);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainMatch("*no existe*");
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId1, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId2, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
    }
} 