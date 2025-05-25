using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class VerificadorStockCachedTests
    {
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly VerificadorStockCached _verificadorStockCached;
        
        public VerificadorStockCachedTests()
        {
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _cacheServiceMock = new Mock<ICacheService>();
            _verificadorStockCached = new VerificadorStockCached(_verificadorStockMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task VerificarYGenerarOrdenesCompraAsync_ShouldUseCache()
        {
            // Arrange
            var resultado = new ResultadoVerificacionStock();
            var cacheKey = "VerificadorStock_VerificarYGenerarOrdenesCompra";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<ResultadoVerificacionStock>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultado);
                
            // Act
            var result = await _verificadorStockCached.VerificarYGenerarOrdenesCompraAsync();
            
            // Assert
            result.Should().BeSameAs(resultado);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<ResultadoVerificacionStock>>>(),
                    It.Is<int>(ttl => ttl == 30), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidatePattern()
        {
            // Arrange
            var cacheKeyPrefix = "VerificadorStock_";
            
            // Act
            _verificadorStockCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
    }
} 