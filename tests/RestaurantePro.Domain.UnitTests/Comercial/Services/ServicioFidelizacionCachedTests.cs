namespace RestaurantePro.Domain.UnitTests.Comercial.Services
{
    public class ServicioFidelizacionCachedTests
    {
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ServicioFidelizacionCached _servicioFidelizacionCached;
        private readonly Guid _clienteId = Guid.NewGuid();
        
        public ServicioFidelizacionCachedTests()
        {
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _cacheServiceMock = new Mock<ICacheService>();
            _servicioFidelizacionCached = new ServicioFidelizacionCached(_servicioFidelizacionMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task CalcularDescuentoAsync_ShouldUseCache()
        {
            // Arrange
            var montoTotal = 100m;
            var resultado = new ResultadoDescuento(10, 10m);
            var cacheKey = $"ServicioFidelizacion_CalcularDescuento_{_clienteId}_{montoTotal}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<ResultadoDescuento>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultado);
                
            // Act
            var result = await _servicioFidelizacionCached.CalcularDescuentoAsync(_clienteId, montoTotal);
            
            // Assert
            result.Should().BeSameAs(resultado);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<ResultadoDescuento>>>(),
                    It.Is<int>(ttl => ttl == 60), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public void CalcularDescuentoPorPuntos_ShouldUseCache()
        {
            // Arrange
            var puntos = 100;
            var montoTotal = 200m;
            var descuentoEsperado = 20m;
            var cacheKey = $"ServicioFidelizacion_CalcularDescuentoPorPuntos_{puntos}_{montoTotal}";
            
            _servicioFidelizacionMock
                .Setup(s => s.CalcularDescuentoPorPuntos(puntos, montoTotal))
                .Returns(descuentoEsperado);
                
            _cacheServiceMock
                .Setup(s => s.GetOrAdd(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<decimal>>(),
                    It.IsAny<int>()))
                .Returns(descuentoEsperado);
                
            // Act
            var result = _servicioFidelizacionCached.CalcularDescuentoPorPuntos(puntos, montoTotal);
            
            // Assert
            result.Should().Be(descuentoEsperado);
            _cacheServiceMock.Verify(
                s => s.GetOrAdd(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<decimal>>(),
                    It.Is<int>(ttl => ttl == 60)), // Verificamos que se use el tiempo de caché correcto
                Times.Once);
        }
        
        [Fact]
        public async Task AcumularPuntosAsync_ShouldInvalidateClientCache()
        {
            // Arrange
            var comandaId = Guid.NewGuid();
            var montoTotal = 100m;
            var clienteCachePattern = $"ServicioFidelizacion_CalcularDescuento_{_clienteId}_";
            
            // Act
            await _servicioFidelizacionCached.AcumularPuntosAsync(_clienteId, comandaId, montoTotal);
            
            // Assert
            _servicioFidelizacionMock.Verify(
                s => s.AcumularPuntosAsync(_clienteId, comandaId, montoTotal),
                Times.Once);
                
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == clienteCachePattern)),
                Times.Once);
        }
        
        [Fact]
        public async Task CanjearPuntosAsync_ShouldInvalidateClientCache()
        {
            // Arrange
            int puntos = 50;
            string concepto = "Test";
            var clienteCachePattern = $"ServicioFidelizacion_CalcularDescuento_{_clienteId}_";
            
            // Act
            await _servicioFidelizacionCached.CanjearPuntosAsync(_clienteId, puntos, concepto);
            
            // Assert
            _servicioFidelizacionMock.Verify(
                s => s.CanjearPuntosAsync(_clienteId, puntos, concepto),
                Times.Once);
                
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == clienteCachePattern)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidatePattern()
        {
            // Arrange
            var cacheKeyPrefix = "ServicioFidelizacion_";
            
            // Act
            _servicioFidelizacionCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
    }
} 