namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class ProductoCategoriaServiceCachedTests
    {
        private readonly Mock<IProductoCategoriaService> _servicioOriginalMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ProductoCategoriaServiceCached _servicioCached;
        private readonly Guid _categoriaId = Guid.NewGuid();
        
        public ProductoCategoriaServiceCachedTests()
        {
            _servicioOriginalMock = new Mock<IProductoCategoriaService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _servicioCached = new ProductoCategoriaServiceCached(_servicioOriginalMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task ObtenerProductosPorCategoriaAsync_ShouldUseCache()
        {
            // Arrange
            bool soloActivos = true;
            var productosEsperados = new List<Producto> { 
                Producto.Crear("Producto 1", "Descripción", new RestaurantePro.Domain.Core.Productos.ValueObjects.PrecioProducto(10.5m), _categoriaId, "Categoría 1") 
            };
            var cacheKey = $"ProductoCategoriaService_ObtenerProductosPorCategoria_{_categoriaId}_{soloActivos}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<List<Producto>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosEsperados);
                
            // Act
            var result = await _servicioCached.ObtenerProductosPorCategoriaAsync(_categoriaId, soloActivos);
            
            // Assert
            result.Should().BeSameAs(productosEsperados);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<List<Producto>>>>(),
                    It.Is<int>(ttl => ttl == 60), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ActualizarCategoriaProductosAsync_ShouldInvalidateCache()
        {
            // Arrange
            var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var nuevaCategoriaId = Guid.NewGuid();
            
            _servicioOriginalMock
                .Setup(s => s.ActualizarCategoriaProductosAsync(productosIds, nuevaCategoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);
                
            // Act
            var result = await _servicioCached.ActualizarCategoriaProductosAsync(productosIds, nuevaCategoriaId);
            
            // Assert
            result.Should().Be(2);
            _servicioOriginalMock.Verify(
                s => s.ActualizarCategoriaProductosAsync(productosIds, nuevaCategoriaId, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se invalida la caché para la categoría
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p.StartsWith($"ProductoCategoriaService_ObtenerProductosPorCategoria_{nuevaCategoriaId}_"))),
                Times.Once);
                
            // Verificar que se invalida la caché al menos una vez (en lugar de verificar por cada producto)
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.IsAny<string>()),
                Times.AtLeastOnce());
        }
        
        [Fact]
        public async Task ReorganizarCategoriasAsync_ShouldInvalidateCache()
        {
            // Arrange
            var nuevosOrdenes = new Dictionary<Guid, int>
            {
                { Guid.NewGuid(), 1 },
                { Guid.NewGuid(), 2 }
            };
            
            _servicioOriginalMock
                .Setup(s => s.ReorganizarCategoriasAsync(nuevosOrdenes, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);
                
            // Act
            var result = await _servicioCached.ReorganizarCategoriasAsync(nuevosOrdenes);
            
            // Assert
            result.Should().Be(2);
            _servicioOriginalMock.Verify(
                s => s.ReorganizarCategoriasAsync(nuevosOrdenes, It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que se invalida la caché para cada categoría
            foreach (var categoriaId in nuevosOrdenes.Keys)
            {
                _cacheServiceMock.Verify(
                    s => s.InvalidatePattern(It.Is<string>(p => p.StartsWith($"ProductoCategoriaService_ObtenerProductosPorCategoria_{categoriaId}_"))),
                    Times.Once);
            }
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidateAllCache()
        {
            // Arrange
            var cacheKeyPrefix = "ProductoCategoriaService_";
            
            // Act
            _servicioCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCacheCategoria_ShouldInvalidateCategoryCache()
        {
            // Arrange
            var cacheKeyPattern = $"ProductoCategoriaService_ObtenerProductosPorCategoria_{_categoriaId}_";
            
            // Act
            _servicioCached.InvalidarCacheCategoria(_categoriaId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPattern)),
                Times.Once);
        }
    }
} 