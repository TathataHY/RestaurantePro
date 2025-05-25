using Moq;
using Xunit;
using FluentAssertions;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using System.Threading.Tasks;
using System;

namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Services
{
    public class MemoryCacheServiceTests
    {
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly MemoryCacheService _cacheService;
        private readonly DateTime _fixedDate = new DateTime(2023, 1, 1, 12, 0, 0);

        public MemoryCacheServiceTests()
        {
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fixedDate);
            _cacheService = new MemoryCacheService(_dateTimeServiceMock.Object);
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
            }, 5); // 5 minutos TTL

            // Avanzar el tiempo más allá del TTL
            _dateTimeServiceMock.Setup(s => s.Now).Returns(_fixedDate.AddMinutes(6));

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
            var removeResult = _cacheService.Remove(key);

            // Act - Intentar recuperar
            _cacheService.GetOrAdd(key, () => {
                callCount++;
                return expectedValue;
            });

            // Assert
            removeResult.Should().BeTrue();
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
            var removedCount = _cacheService.InvalidatePattern(prefix);

            // Act - Verificar si siguen en caché
            int callCount = 0;
            _cacheService.GetOrAdd(key1, () => { callCount++; return "new-value1"; });
            _cacheService.GetOrAdd(key2, () => { callCount++; return "new-value2"; });
            _cacheService.GetOrAdd(key3, () => { callCount++; return "new-value3"; });

            // Assert
            removedCount.Should().Be(2); // Debe eliminar 2 claves
            callCount.Should().Be(2); // Solo debe recargar las 2 claves invalidadas
        }

        [Fact]
        public void Clear_ShouldRemoveAllValues()
        {
            // Arrange
            _cacheService.GetOrAdd("key1", () => "value1");
            _cacheService.GetOrAdd("key2", () => "value2");

            // Act
            _cacheService.Clear();

            // Act - Verificar si siguen en caché
            int callCount = 0;
            _cacheService.GetOrAdd("key1", () => { callCount++; return "new-value1"; });
            _cacheService.GetOrAdd("key2", () => { callCount++; return "new-value2"; });

            // Assert
            callCount.Should().Be(2); // Debe recargar ambas claves
        }
    }
} 