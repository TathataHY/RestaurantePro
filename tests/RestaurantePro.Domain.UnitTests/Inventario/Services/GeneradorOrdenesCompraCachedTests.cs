namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class GeneradorOrdenesCompraCachedTests
    {
        private readonly Mock<IGeneradorOrdenesCompra> _servicioOriginalMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GeneradorOrdenesCompraCached _servicioCached;
        private readonly Guid _ingredienteId = Guid.NewGuid();
        
        public GeneradorOrdenesCompraCachedTests()
        {
            _servicioOriginalMock = new Mock<IGeneradorOrdenesCompra>();
            _cacheServiceMock = new Mock<ICacheService>();
            _servicioCached = new GeneradorOrdenesCompraCached(_servicioOriginalMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task GenerarOrdenesCompraAutomaticas_ShouldUseCache()
        {
            // Arrange
            var ordenesEsperadas = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var cacheKey = "GeneradorOrdenesCompra_GenerarOrdenesCompraAutomaticas";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<IEnumerable<Guid>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesEsperadas);
                
            // Act
            var result = await _servicioCached.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            result.Should().BeSameAs(ordenesEsperadas);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<IEnumerable<Guid>>>>(),
                    It.Is<int>(ttl => ttl == 15), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_ShouldUseCache()
        {
            // Arrange
            Guid? ordenEsperada = Guid.NewGuid();
            var cacheKey = $"GeneradorOrdenesCompra_GenerarOrdenCompraParaIngrediente_{_ingredienteId}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Guid?>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenEsperada);
                
            // Act
            var result = await _servicioCached.GenerarOrdenCompraParaIngrediente(_ingredienteId);
            
            // Assert
            result.Should().Be(ordenEsperada);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Guid?>>>(),
                    It.Is<int>(ttl => ttl == 15),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidateAllCache()
        {
            // Arrange
            var cacheKeyPrefix = "GeneradorOrdenesCompra_";
            
            // Act
            _servicioCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCachePorIngrediente_ShouldInvalidateIngredienteCache()
        {
            // Arrange
            var cacheKeyPattern = $"GeneradorOrdenesCompra_GenerarOrdenCompraParaIngrediente_{_ingredienteId}";
            
            // Act
            _servicioCached.InvalidarCachePorIngrediente(_ingredienteId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPattern)),
                Times.Once);
        }
    }
} 