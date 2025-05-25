namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services.Cache
{
    public class MemoryCacheServiceTests
    {
        private readonly MemoryCacheService _cacheService;

        public MemoryCacheServiceTests()
        {
            _cacheService = new MemoryCacheService();
        }

        [Fact]
        public void GetOrAdd_ShouldAddValueToCache_WhenNotExists()
        {
            // Arrange
            const string key = "test-key";
            const string expectedValue = "test-value";
            int callCount = 0;

            // Act
            var result1 = _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue;
            });
            
            var result2 = _cacheService.GetOrAdd(key, () => {
                callCount++;
                return "different-value";
            });

            // Assert
            result1.Should().Be(expectedValue);
            result2.Should().Be(expectedValue); // Debe devolver el mismo valor
            callCount.Should().Be(1); // La función de carga solo debe llamarse una vez
        }

        [Fact]
        public void GetOrAdd_ShouldReloadValue_WhenExpired()
        {
            // Arrange
            const string key = "test-key";
            const string expectedValue1 = "test-value-1";
            const string expectedValue2 = "test-value-2";
            int callCount = 0;

            // Act - Primera carga
            var result1 = _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue1;
            }, 0); // 0 minutos TTL para forzar expiración inmediata

            // Act - Segunda carga (después de expirar)
            var result2 = _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue2;
            }, 5);

            // Assert
            result1.Should().Be(expectedValue1);
            result2.Should().Be(expectedValue2); // Debe cargar el nuevo valor
            callCount.Should().Be(2); // La función de carga debe llamarse dos veces
        }

        [Fact]
        public async Task GetOrAddAsync_ShouldAddValueToCache_WhenNotExists()
        {
            // Arrange
            const string key = "test-key-async";
            const string expectedValue = "test-value-async";
            int callCount = 0;

            // Act
            var result1 = await _cacheService.GetOrAddAsync(key, 
                (cancellationToken) => {
                    callCount++;
                    return Task.FromResult(expectedValue);
                });
            
            var result2 = await _cacheService.GetOrAddAsync(key, 
                (cancellationToken) => {
                    callCount++;
                    return Task.FromResult("different-value");
                });

            // Assert
            result1.Should().Be(expectedValue);
            result2.Should().Be(expectedValue); // Debe devolver el mismo valor
            callCount.Should().Be(1); // La función de carga solo debe llamarse una vez
        }

        [Fact]
        public void Remove_ShouldRemoveValueFromCache()
        {
            // Arrange
            const string key = "test-key-remove";
            const string expectedValue = "test-value-remove";
            int callCount = 0;

            // Act - Agregar a caché
            _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue;
            });

            // Act - Eliminar de caché
            _cacheService.Remove(key);

            // Act - Intentar recuperar
            _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue;
            });

            // Assert
            callCount.Should().Be(2); // La función de carga debe llamarse dos veces
        }

        [Fact]
        public void InvalidatePattern_ShouldRemoveMatchingKeys()
        {
            // Arrange
            const string prefix = "prefix-";
            const string key1 = "prefix-key1";
            const string key2 = "prefix-key2";
            const string key3 = "other-key";

            // Act - Agregar a caché
            _cacheService.GetOrAdd(key1, () => "value1");
            _cacheService.GetOrAdd(key2, () => "value2");
            _cacheService.GetOrAdd(key3, () => "value3");

            // Act - Invalidar por patrón
            _cacheService.InvalidatePattern(prefix);

            // Act - Verificar si siguen en caché
            int callCount = 0;
            _cacheService.GetOrAdd(key1, () => { callCount++; return "new-value1"; });
            _cacheService.GetOrAdd(key2, () => { callCount++; return "new-value2"; });
            _cacheService.GetOrAdd(key3, () => { callCount++; return "new-value3"; });

            // Assert
            callCount.Should().Be(2); // Solo debe recargar las 2 claves invalidadas
        }
    }
} 