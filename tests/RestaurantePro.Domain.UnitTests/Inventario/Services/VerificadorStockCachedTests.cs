using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Inventario.Results;

namespace RestaurantePro.Domain.UnitTests.Inventario.Services
{
    public class VerificadorStockCachedTests
    {
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly VerificadorStockCached _verificadorStockCached;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;
        
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
            var resultSuccess = Result.Success(resultado);
            var cacheKey = "VerificadorStock_VerificarYGenerarOrdenesCompra";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<ResultadoVerificacionStock>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(resultSuccess);
                
            // Act
            var result = await _verificadorStockCached.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeSameAs(resultado);
            
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Result<ResultadoVerificacionStock>>>>(),
                    It.Is<int>(ttl => ttl == 30), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificamos que no se llamó directamente al servicio original
            _verificadorStockMock.Verify(
                s => s.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()),
                Times.Never);
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
        
        [Fact]
        public async Task VerificarYGenerarOrdenesCompraAsync_WhenCacheThrowsException_ShouldUseOriginalService()
        {
            // Arrange
            var expectedResultado = new ResultadoVerificacionStock();
            var expectedResult = Result.Success(expectedResultado);
            
            // Configurar el error en la caché
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<ResultadoVerificacionStock>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error simulado en la caché"));
                
            // Configurar el servicio original
            _verificadorStockMock
                .Setup(s => s.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
                
            // Act
            var result = await _verificadorStockCached.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeSameAs(expectedResultado);
            
            // Verificar que se usó el servicio original como fallback
            _verificadorStockMock.Verify(
                s => s.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task VerificarYGenerarOrdenesCompraAsync_WhenOriginalServiceFails_ShouldPropagateFailure()
        {
            // Arrange
            var errorMessage = "Error de verificación de stock";
            var expectedResult = Result.Failure<ResultadoVerificacionStock>(errorMessage);
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<ResultadoVerificacionStock>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);
                
            // Act
            var result = await _verificadorStockCached.VerificarYGenerarOrdenesCompraAsync(_cancellationToken);
            
            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Be(errorMessage);
        }
    }
} 