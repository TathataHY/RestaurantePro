namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class GeneradorOrdenesCompraCachedTests
    {
        private readonly Mock<IGeneradorOrdenesCompra> _servicioOriginalMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GeneradorOrdenesCompraCached _servicioCached;
        private readonly Guid _ingredienteId = Guid.NewGuid();
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
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
            var resultadoEsperado = Result.Success<IEnumerable<Guid>>(ordenesEsperadas);
            var cacheKey = "GeneradorOrdenesCompra_GenerarOrdenesCompraAutomaticas";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<IEnumerable<Guid>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoEsperado);
                
            // Act
            var result = await _servicioCached.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(ordenesEsperadas);
            
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<IEnumerable<Guid>>>>>(),
                    It.Is<int>(ttl => ttl == 15), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que no se llamó al servicio original
            _servicioOriginalMock.Verify(
                s => s.GenerarOrdenesCompraAutomaticas(It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_ShouldUseCache()
        {
            // Arrange
            Guid? ordenEsperada = Guid.NewGuid();
            var resultadoEsperado = Result.Success<Guid?>(ordenEsperada);
            var cacheKey = $"GeneradorOrdenesCompra_GenerarOrdenCompraParaIngrediente_{_ingredienteId}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<Guid?>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoEsperado);
                
            // Act
            var result = await _servicioCached.GenerarOrdenCompraParaIngrediente(_ingredienteId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(ordenEsperada);
            
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<Guid?>>>>(),
                    It.Is<int>(ttl => ttl == 15),
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que no se llamó al servicio original
            _servicioOriginalMock.Verify(
                s => s.GenerarOrdenCompraParaIngrediente(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
        
        [Fact]
        public async Task GenerarOrdenesCompraAutomaticas_WhenCacheThrowsException_ShouldUseOriginalService()
        {
            // Arrange
            var ordenesEsperadas = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var resultadoEsperado = Result.Success<IEnumerable<Guid>>(ordenesEsperadas);
            
            // Configurar error en la caché
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<IEnumerable<Guid>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado en la caché"));
                
            // Configurar servicio original
            _servicioOriginalMock
                .Setup(s => s.GenerarOrdenesCompraAutomaticas(It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoEsperado);
                
            // Act
            var result = await _servicioCached.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(ordenesEsperadas);
            
            // Verificar que se llamó al servicio original como fallback
            _servicioOriginalMock.Verify(
                s => s.GenerarOrdenesCompraAutomaticas(It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_WhenCacheThrowsException_ShouldUseOriginalService()
        {
            // Arrange
            Guid? ordenEsperada = Guid.NewGuid();
            var resultadoEsperado = Result.Success<Guid?>(ordenEsperada);
            
            // Configurar error en la caché
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<Guid?>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado en la caché"));
                
            // Configurar servicio original
            _servicioOriginalMock
                .Setup(s => s.GenerarOrdenCompraParaIngrediente(_ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoEsperado);
                
            // Act
            var result = await _servicioCached.GenerarOrdenCompraParaIngrediente(_ingredienteId);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().Be(ordenEsperada);
            
            // Verificar que se llamó al servicio original como fallback
            _servicioOriginalMock.Verify(
                s => s.GenerarOrdenCompraParaIngrediente(_ingredienteId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task GenerarOrdenCompraParaIngrediente_ConIdVacio_DebeRetornarError()
        {
            // Arrange
            var ingredienteIdVacio = Guid.Empty;
            
            // Act
            var result = await _servicioCached.GenerarOrdenCompraParaIngrediente(ingredienteIdVacio);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("vacío");
            
            // Verificar que no se llamó a la caché ni al servicio original
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<Guid?>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
                
            _servicioOriginalMock.Verify(
                s => s.GenerarOrdenCompraParaIngrediente(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task GenerarOrdenesCompraAutomaticas_WhenFailure_ShouldPropagateError()
        {
            // Arrange
            var errorMensaje = "Error al generar órdenes de compra";
            var resultadoError = Result.Failure<IEnumerable<Guid>>(errorMensaje);
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<IEnumerable<Guid>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultadoError);
                
            // Act
            var result = await _servicioCached.GenerarOrdenesCompraAutomaticas();
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be(errorMensaje);
        }
    }
} 