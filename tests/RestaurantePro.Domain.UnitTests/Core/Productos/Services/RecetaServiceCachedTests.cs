using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Core.Productos.Services;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceCachedTests
    {
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly RecetaServiceCached _recetaServiceCached;
        private readonly Guid _productoId = Guid.NewGuid();
        
        public RecetaServiceCachedTests()
        {
            _recetaServiceMock = new Mock<IRecetaService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _recetaServiceCached = new RecetaServiceCached(_recetaServiceMock.Object, _cacheServiceMock.Object);
        }
        
        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ShouldUseCache()
        {
            // Arrange
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 2.5m },
                { Guid.NewGuid(), 1.0m }
            };
            var cacheKey = $"RecetaService_ObtenerIngredientesParaProducto_{_productoId}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Dictionary<Guid, decimal>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes);
                
            // Act
            var result = await _recetaServiceCached.ObtenerIngredientesParaProductoAsync(_productoId);
            
            // Assert
            result.Should().BeSameAs(ingredientes);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Dictionary<Guid, decimal>>>>(),
                    It.Is<int>(ttl => ttl == 60), // Verificamos que se use el tiempo de caché correcto
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_ShouldNotUseCache()
        {
            // Arrange
            int cantidad = 5;
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(_productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            // Act
            var result = await _recetaServiceCached.VerificarDisponibilidadIngredientesAsync(_productoId, cantidad);
            
            // Assert
            result.Should().BeTrue();
            _recetaServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesAsync(_productoId, cantidad, It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<bool>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_ShouldNotUseCache()
        {
            // Arrange
            int cantidad = 5;
            var faltantes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 1.5m }
            };
            
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesFaltantesAsync(_productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(faltantes);
                
            // Act
            var result = await _recetaServiceCached.ObtenerIngredientesFaltantesAsync(_productoId, cantidad);
            
            // Assert
            result.Should().BeSameAs(faltantes);
            _recetaServiceMock.Verify(
                s => s.ObtenerIngredientesFaltantesAsync(_productoId, cantidad, It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Dictionary<Guid, decimal>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task CalcularCostoRecetaAsync_ShouldUseCache()
        {
            // Arrange
            decimal costoReceta = 150.50m;
            var cacheKey = $"RecetaService_CalcularCostoReceta_{_productoId}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<decimal>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(costoReceta);
                
            // Act
            var result = await _recetaServiceCached.CalcularCostoRecetaAsync(_productoId);
            
            // Assert
            result.Should().Be(costoReceta);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<decimal>>>(),
                    It.Is<int>(ttl => ttl == 60),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ShouldUseCache()
        {
            // Arrange
            var rentabilidad = Domain.Core.Productos.ValueObjects.RentabilidadProducto.Calcular(50m, 100m);
            var cacheKey = $"RecetaService_CalcularRentabilidadProducto_{_productoId}";
            
            _cacheServiceMock
                .Setup(s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Domain.Core.Productos.ValueObjects.RentabilidadProducto>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(rentabilidad);
                
            // Act
            var result = await _recetaServiceCached.CalcularRentabilidadProductoAsync(_productoId);
            
            // Assert
            result.Should().BeSameAs(rentabilidad);
            _cacheServiceMock.Verify(
                s => s.GetOrAddAsync(
                    It.Is<string>(k => k == cacheKey),
                    It.IsAny<Func<CancellationToken, Task<Domain.Core.Productos.ValueObjects.RentabilidadProducto>>>(),
                    It.Is<int>(ttl => ttl == 60),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCache_ShouldInvalidatePattern()
        {
            // Arrange
            var cacheKeyPrefix = "RecetaService_";
            
            // Act
            _recetaServiceCached.InvalidarCache();
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == cacheKeyPrefix)),
                Times.Once);
        }
        
        [Fact]
        public void InvalidarCacheProducto_ShouldInvalidateProductRelatedEntries()
        {
            // Arrange
            var ingredientesCacheKey = $"RecetaService_ObtenerIngredientesParaProducto_{_productoId}";
            var costoCacheKey = $"RecetaService_CalcularCostoReceta_{_productoId}";
            var rentabilidadCacheKey = $"RecetaService_CalcularRentabilidadProducto_{_productoId}";
            
            // Act
            _recetaServiceCached.InvalidarCacheProducto(_productoId);
            
            // Assert
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == ingredientesCacheKey)),
                Times.Once);
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == costoCacheKey)),
                Times.Once);
            _cacheServiceMock.Verify(
                s => s.InvalidatePattern(It.Is<string>(p => p == rentabilidadCacheKey)),
                Times.Once);
        }
    }
} 