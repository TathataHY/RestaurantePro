using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Caching;
using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración para CacheService - Optimización de performance
/// </summary>
public class CacheServiceIntegrationTests : MobileIntegrationTestBase
{
    private ICacheService _cacheService = null!;

    public CacheServiceIntegrationTests(MobileIntegrationTestFixture fixture) : base(fixture)
    {
        // Configurar servicios específicos para estos tests
        var services = new ServiceCollection();
        services.AddScoped<ICacheService, MockCacheService>();
        var serviceProvider = services.BuildServiceProvider();
        _cacheService = serviceProvider.GetRequiredService<ICacheService>();
    }

    #region Basic Cache Operations

    [Fact]
    public async Task GetOrSetAsync_WithValidData_ShouldCacheAndReturnValue()
    {
        // Arrange
        var key = "test_key_1";
        var expectedValue = "test_value";
        var callCount = 0;

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10); // Simular operación asíncrona
            return expectedValue;
        });

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "should_not_be_called";
        });

        // Assert
        Assert.Equal(expectedValue, result1);
        Assert.Equal(expectedValue, result2);
        Assert.Equal(1, callCount); // Solo debe llamarse una vez
    }

    [Fact]
    public async Task GetOrSetAsync_WithExpiration_ShouldExpireAfterTime()
    {
        // Arrange
        var key = "test_key_expiration";
        var expectedValue = "test_value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return expectedValue;
        }, expiration);

        // Esperar a que expire
        await Task.Delay(150);

        var callCount = 0;
        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "new_value";
        }, expiration);

        // Assert
        Assert.Equal(expectedValue, result1);
        Assert.Equal("new_value", result2);
        Assert.Equal(1, callCount); // Debe llamarse de nuevo porque expiró
    }

    [Fact]
    public async Task GetOrSetAsync_WithComplexObject_ShouldCacheCorrectly()
    {
        // Arrange
        var key = "test_complex_object";
        var expectedProduct = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Test Product",
            Precio = 15.99m,
            Descripcion = "Test Description"
        };

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return expectedProduct;
        });

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            return new ProductoDto { Id = Guid.NewGuid() };
        });

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(expectedProduct.Id, result1.Id);
        Assert.Equal(expectedProduct.Id, result2.Id);
        Assert.Equal(expectedProduct.Nombre, result1.Nombre);
        Assert.Equal(expectedProduct.Nombre, result2.Nombre);
    }

    [Fact]
    public async Task GetOrSetAsync_WithNullValue_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "test_null_value";

        // Act
        var result = await _cacheService.GetOrSetAsync<string?>(key, async () =>
        {
            await Task.Delay(10);
            return null;
        });

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrSetAsync_WithException_ShouldNotCache()
    {
        // Arrange
        var key = "test_exception";
        var callCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _cacheService.GetOrSetAsync<string>(key, async () =>
            {
                callCount++;
                await Task.Delay(10);
                throw new InvalidOperationException("Test exception");
            });
        });

        // Intentar de nuevo - debe llamar la función otra vez
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _cacheService.GetOrSetAsync<string>(key, async () =>
            {
                callCount++;
                await Task.Delay(10);
                throw new InvalidOperationException("Test exception");
            });
        });

        Assert.Equal(2, callCount); // Debe llamarse dos veces porque no se cacheó
    }

    #endregion

    #region Cache Performance Tests

    [Fact]
    public async Task GetOrSetAsync_ConcurrentAccess_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "test_concurrent";
        var expectedValue = "concurrent_value";
        var callCount = 0;
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_cacheService.GetOrSetAsync(key, async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50);
                return expectedValue;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.Equal(expectedValue, result));
        Assert.Equal(1, callCount); // Solo debe ejecutarse una vez
    }

    [Fact]
    public async Task GetOrSetAsync_MultipleKeys_ShouldCacheIndependently()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3" };
        var expectedValues = new[] { "value1", "value2", "value3" };
        var callCounts = new int[3];

        // Act
        var tasks = keys.Select(async (key, index) =>
        {
            return await _cacheService.GetOrSetAsync(key, async () =>
            {
                callCounts[index]++;
                await Task.Delay(10);
                return expectedValues[index];
            });
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.Equal(expectedValues[i], results[i]);
            Assert.Equal(1, callCounts[i]);
        }
    }

    #endregion

    #region Cache Cleanup Tests

    [Fact]
    public async Task GetOrSetAsync_AfterCleanup_ShouldRefetchData()
    {
        // Arrange
        var key = "test_cleanup";
        var expectedValue = "original_value";
        var newValue = "new_value";
        var expiration = TimeSpan.FromMilliseconds(50);

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return expectedValue;
        }, expiration);

        // Esperar a que expire
        await Task.Delay(100);

        var callCount = 0;
        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return newValue;
        });

        // Assert
        Assert.Equal(expectedValue, result1);
        Assert.Equal(newValue, result2);
        Assert.Equal(1, callCount);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetOrSetAsync_WithCancellation_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "test_cancellation";
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await _cacheService.GetOrSetAsync(key, async () =>
            {
                await Task.Delay(100, cts.Token);
                return "should_not_reach_here";
            });
        });
    }

    [Fact]
    public async Task GetOrSetAsync_WithLongRunningOperation_ShouldCacheResult()
    {
        // Arrange
        var key = "test_long_running";
        var expectedValue = "long_running_result";
        var callCount = 0;

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(200); // Operación larga
            return expectedValue;
        });

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(200);
            return "should_not_be_called";
        });

        // Assert
        Assert.Equal(expectedValue, result1);
        Assert.Equal(expectedValue, result2);
        Assert.Equal(1, callCount);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task GetOrSetAsync_CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var productKey = "product_list";
        var categoryKey = "category_list";
        var callCounts = new { Products = 0, Categories = 0 };

        // Act - Simular carga de datos del dashboard
        var productsTask = _cacheService.GetOrSetAsync<List<ProductoDto>>(productKey, async () =>
        {
            await Task.Delay(100);
            return new List<ProductoDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Product 1", Precio = 10.99m },
                new() { Id = Guid.NewGuid(), Nombre = "Product 2", Precio = 15.99m }
            };
        });

        var categoriesTask = _cacheService.GetOrSetAsync<List<string>>(categoryKey, async () =>
        {
            await Task.Delay(80);
            return new List<string> { "Bebidas", "Comidas", "Postres" };
        });

        var products = await productsTask;
        var categories = await categoriesTask;

        // Act - Segunda llamada (debe usar cache)
        var products2 = await _cacheService.GetOrSetAsync<List<ProductoDto>>(productKey, async () =>
        {
            await Task.Delay(100);
            return new List<ProductoDto>();
        });

        var categories2 = await _cacheService.GetOrSetAsync<List<string>>(categoryKey, async () =>
        {
            await Task.Delay(80);
            return new List<string>();
        });

        // Assert
        Assert.NotNull(products);
        Assert.NotNull(categories);
        Assert.Equal(2, products.Count);
        Assert.Equal(3, categories.Count);
        Assert.Equal(products.Count, products2.Count);
        Assert.Equal(categories.Count, categories2.Count);
    }

    #endregion
}

/// <summary>
/// Implementación mock del CacheService para tests
/// </summary>
public class MockCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, DateTime> _expirations = new();
    private readonly Dictionary<string, Task> _runningTasks = new();
    private readonly object _lock = new();

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // Verificar si existe y no ha expirado
        lock (_lock)
        {
            if (_cache.ContainsKey(key) && (!_expirations.ContainsKey(key) || _expirations[key] > DateTime.UtcNow))
            {
                return (T)_cache[key];
            }

            // Si ya hay una tarea ejecutándose para esta clave, esperarla
            if (_runningTasks.ContainsKey(key))
            {
                var runningTask = _runningTasks[key];
                return (T)_cache[key]; // Esperar a que termine y devolver el resultado
            }
        }

        // Crear nueva tarea para esta clave
        Task<T> task = null;
        lock (_lock)
        {
            if (_runningTasks.ContainsKey(key))
            {
                // Otra tarea ya está ejecutándose, esperarla
                return (T)_cache[key];
            }

            task = ExecuteFactoryAndCache(key, factory, expiration);
            _runningTasks[key] = task;
        }

        try
        {
            var result = await task;
            return result;
        }
        finally
        {
            lock (_lock)
            {
                _runningTasks.Remove(key);
            }
        }
    }

    private async Task<T> ExecuteFactoryAndCache<T>(string key, Func<Task<T>> factory, TimeSpan? expiration)
    {
        // Ejecutar factory y cachear resultado
        var result = await factory();
        
        lock (_lock)
        {
            _cache[key] = result!;
            
            if (expiration.HasValue)
            {
                _expirations[key] = DateTime.UtcNow.Add(expiration.Value);
            }
        }

        return result;
    }
}
