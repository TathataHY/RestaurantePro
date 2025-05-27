namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceCachedTests
    {
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly RecetaServiceCached _sut;

        public RecetaServiceCachedTests()
        {
            _recetaServiceMock = new Mock<IRecetaService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _notificationManager = new NotificationManager();

            _sut = new RecetaServiceCached(
                _recetaServiceMock.Object,
                _cacheServiceMock.Object,
                _notificationManager);
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ConProductoValido_DebeRetornarDesdeCache()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 100 },
                { Guid.NewGuid(), 200 }
            };

            var expectedResult = Result.Success(ingredientes);

            _cacheServiceMock
                .Setup(c => c.GetOrAddAsync(
                    It.Is<string>(s => s.Contains(productoId.ToString())),
                    It.IsAny<Func<CancellationToken, Task<Result<Dictionary<Guid, decimal>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.ObtenerIngredientesParaProductoAsync(productoId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(ingredientes);

            // Verificar que se usó la caché
            _cacheServiceMock.Verify(
                c => c.GetOrAddAsync(
                    It.Is<string>(s => s.Contains(productoId.ToString())),
                    It.IsAny<Func<CancellationToken, Task<Result<Dictionary<Guid, decimal>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que no se llamó directamente al servicio original
            _recetaServiceMock.Verify(
                s => s.ObtenerIngredientesParaProductoAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ConIdVacio_DebeRetornarError()
        {
            // Arrange
            var productoId = Guid.Empty;

            // Mock de un resultado fallido con un mensaje de error específico
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesParaProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<Dictionary<Guid, decimal>>("El ID del producto no puede estar vacío"));

            // Act
            var result = await _sut.ObtenerIngredientesParaProductoAsync(productoId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("producto");
            
            // Verificar que no se usó la caché
            _cacheServiceMock.Verify(
                c => c.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<Dictionary<Guid, decimal>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ConErrorEnCache_DebeUsarServicioOriginal()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 100 },
                { Guid.NewGuid(), 200 }
            };

            var expectedResult = Result.Success(ingredientes);

            // Configurar el error en la caché
            _cacheServiceMock
                .Setup(c => c.GetOrAddAsync(
                    It.Is<string>(s => s.Contains(productoId.ToString())),
                    It.IsAny<Func<CancellationToken, Task<Result<Dictionary<Guid, decimal>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error de caché"));

            // Configurar el servicio original para retornar datos
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesParaProductoAsync(
                    productoId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.ObtenerIngredientesParaProductoAsync(productoId);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(ingredientes);

            // Verificar que se intentó usar la caché
            _cacheServiceMock.Verify(
                c => c.GetOrAddAsync(
                    It.Is<string>(s => s.Contains(productoId.ToString())),
                    It.IsAny<Func<CancellationToken, Task<Result<Dictionary<Guid, decimal>>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que se llamó al servicio original
            _recetaServiceMock.Verify(
                s => s.ObtenerIngredientesParaProductoAsync(
                    productoId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_ConParametrosValidos_DebeUsarServicioOriginal()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var expectedResult = Result.Success(true);

            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId,
                    cantidad,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _sut.VerificarDisponibilidadIngredientesAsync(productoId, cantidad);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Value.Should().BeTrue();

            // Verificar que se llamó al servicio original
            _recetaServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId,
                    cantidad,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verificar que no se usó la caché (este método no usa caché)
            _cacheServiceMock.Verify(
                c => c.GetOrAddAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<CancellationToken, Task<Result<bool>>>>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_ConCantidadInvalida_DebeRetornarError()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0; // Cantidad inválida

            // Mock de un resultado fallido con un mensaje de error específico
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<bool>("La cantidad debe ser mayor a cero"));

            // Act
            var result = await _sut.VerificarDisponibilidadIngredientesAsync(productoId, cantidad);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeFalse();
            result.Error.Should().Contain("cantidad");
            
            // Verificar que se llamó al servicio original
            _recetaServiceMock.Verify(
                s => s.VerificarDisponibilidadIngredientesAsync(
                    productoId,
                    cantidad,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void InvalidarCacheProducto_ConProductoValido_DebeInvalidarTodasLasEntradasRelacionadas()
        {
            // Arrange
            var productoId = Guid.NewGuid();

            // Act
            _sut.InvalidarCacheProducto(productoId);

            // Assert
            // Verificar que se llamó a invalidar para cada tipo de caché relacionado con el producto
            _cacheServiceMock.Verify(
                c => c.InvalidatePattern(It.Is<string>(s => s.Contains("ObtenerIngredientesParaProducto") && s.Contains(productoId.ToString()))),
                Times.Once);

            _cacheServiceMock.Verify(
                c => c.InvalidatePattern(It.Is<string>(s => s.Contains("CalcularCostoReceta") && s.Contains(productoId.ToString()))),
                Times.Once);

            _cacheServiceMock.Verify(
                c => c.InvalidatePattern(It.Is<string>(s => s.Contains("CalcularRentabilidadProducto") && s.Contains(productoId.ToString()))),
                Times.Once);
        }

        [Fact]
        public void InvalidarCacheProducto_ConIdVacio_NoDebeInvalidarNada()
        {
            // Arrange
            var productoId = Guid.Empty;

            // Act
            _sut.InvalidarCacheProducto(productoId);

            // Assert
            // Verificar que no se llamó a invalidar nada
            _cacheServiceMock.Verify(
                c => c.InvalidatePattern(It.IsAny<string>()),
                Times.Never);
        }
    }
} 