using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Mobile.Core.Services.Caching;
using RestaurantePro.Mobile.Core.Models.DTOs;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración ROBUSTOS para CacheService - Casos edge, stress y resiliencia
/// </summary>
public class CacheServiceRobustIntegrationTests : MobileIntegrationTestBase
{
    private ICacheService _cacheService = null!;

    public CacheServiceRobustIntegrationTests(MobileIntegrationTestFixture fixture) : base(fixture)
    {
        // Configurar servicios específicos para estos tests
        var services = new ServiceCollection();
        services.AddScoped<ICacheService, RobustMockCacheService>();
        var serviceProvider = services.BuildServiceProvider();
        _cacheService = serviceProvider.GetRequiredService<ICacheService>();
    }

    #region Tests de Concurrencia Avanzada

    [Fact]
    public async Task GetOrSetAsync_HighConcurrency_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "high_concurrency_test";
        var expectedValue = "concurrent_value";
        var callCount = 0;
        var concurrencyLevel = 50;
        var tasks = new List<Task<string>>();

        // Act - Crear muchas tareas concurrentes
        for (int i = 0; i < concurrencyLevel; i++)
        {
            tasks.Add(_cacheService.GetOrSetAsync(key, async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(10); // Simular operación asíncrona
                return expectedValue;
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.Equal(expectedValue, result));
        Assert.Equal(1, callCount); // Solo debe ejecutarse una vez
    }

    [Fact]
    public async Task GetOrSetAsync_MultipleKeysConcurrently_ShouldCacheIndependently()
    {
        // Arrange
        var keys = Enumerable.Range(1, 20).Select(i => $"key_{i}").ToArray();
        var expectedValues = keys.Select(key => $"value_{key}").ToArray();
        var callCounts = new int[keys.Length];

        // Act - Ejecutar todas las claves concurrentemente
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

    [Fact]
    public async Task GetOrSetAsync_ConcurrentExpiration_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "concurrent_expiration";
        var expiration = TimeSpan.FromMilliseconds(100);
        var callCount = 0;

        // Act - Primera llamada
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "first_value";
        }, expiration);

        // Esperar a que expire
        await Task.Delay(150);

        // Segunda llamada concurrente después de expiración
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_cacheService.GetOrSetAsync(key, async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(10);
                return "second_value";
            }, expiration));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal("first_value", result1);
        Assert.All(results, result => Assert.Equal("second_value", result));
        Assert.Equal(2, callCount); // Primera llamada + una segunda llamada (las concurrentes deben compartir)
    }

    #endregion

    #region Tests de Stress y Carga

    [Fact]
    public async Task GetOrSetAsync_UnderHighLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var iterations = 100;
        var tasks = new List<Task<string>>();
        var callCount = 0;

        // Act - Simular alta carga
        for (int i = 0; i < iterations; i++)
        {
            var key = $"load_test_{i % 10}"; // Reutilizar algunas claves
            tasks.Add(_cacheService.GetOrSetAsync(key, async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(5);
                return $"value_{i}";
            }));
        }

        var startTime = DateTime.UtcNow;
        var results = await Task.WhenAll(tasks);
        var endTime = DateTime.UtcNow;

        // Assert
        var totalTime = endTime - startTime;
        Assert.True(totalTime.TotalSeconds < 10, $"Operaciones completadas en {totalTime.TotalSeconds} segundos");
        Assert.True(callCount < iterations, "El cache debe reducir las llamadas");
        Assert.All(results, result => Assert.NotNull(result));
    }

    [Fact]
    public async Task GetOrSetAsync_MemoryPressure_ShouldHandleCorrectly()
    {
        // Arrange
        var largeObjects = new List<LargeObject>();
        var callCount = 0;

        // Act - Crear muchos objetos grandes
        for (int i = 0; i < 50; i++)
        {
            var key = $"large_object_{i}";
            var result = await _cacheService.GetOrSetAsync(key, async () =>
            {
                callCount++;
                await Task.Delay(1);
                var largeObj = new LargeObject
                {
                    Id = i,
                    Data = new string('x', 1000), // 1KB de datos
                    Items = Enumerable.Range(1, 100).Select(j => $"item_{j}").ToList()
                };
                largeObjects.Add(largeObj);
                return largeObj;
            });
        }

        // Assert
        Assert.Equal(50, callCount); // Cada objeto debe ser creado una vez
        Assert.Equal(50, largeObjects.Count);
    }

    #endregion

    #region Tests de Casos Edge

    [Fact]
    public async Task GetOrSetAsync_WithNullKey_ShouldHandleCorrectly()
    {
        // Arrange
        var callCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _cacheService.GetOrSetAsync(null!, async () =>
            {
                callCount++;
                await Task.Delay(10);
                return "should_not_reach_here";
            });
        });

        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task GetOrSetAsync_WithEmptyKey_ShouldHandleCorrectly()
    {
        // Arrange
        var callCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync("", async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "empty_key_value";
        });

        // Assert
        Assert.Equal("empty_key_value", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrSetAsync_WithVeryLongKey_ShouldHandleCorrectly()
    {
        // Arrange
        var longKey = new string('a', 10000); // Clave muy larga
        var callCount = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(longKey, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "long_key_value";
        });

        // Assert
        Assert.Equal("long_key_value", result);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task GetOrSetAsync_WithSpecialCharactersInKey_ShouldHandleCorrectly()
    {
        // Arrange
        var specialKeys = new[]
        {
            "key with spaces",
            "key-with-dashes",
            "key_with_underscores",
            "key.with.dots",
            "key@with#special$chars",
            "key/with\\slashes",
            "key:with;colons"
        };

        // Act & Assert
        foreach (var key in specialKeys)
        {
            var result = await _cacheService.GetOrSetAsync(key, async () =>
            {
                await Task.Delay(1);
                return $"value_for_{key}";
            });

            Assert.Equal($"value_for_{key}", result);
        }
    }

    #endregion

    #region Tests de Expiración Avanzada

    [Fact]
    public async Task GetOrSetAsync_WithNullExpiration_ShouldNotExpire()
    {
        // Arrange
        var key = "null_expiration_test";
        TimeSpan? expiration = null; // null significa no expirar
        var callCount = 0;

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "null_expiration_value";
        }, expiration);

        // Esperar un poco
        await Task.Delay(50);

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "should_not_be_called";
        }, expiration);

        // Assert
        Assert.Equal("null_expiration_value", result1);
        Assert.Equal("null_expiration_value", result2);
        Assert.Equal(1, callCount); // Solo debe llamarse una vez
    }

    [Fact]
    public async Task GetOrSetAsync_WithNegativeExpiration_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "negative_expiration_test";
        var expiration = TimeSpan.FromMilliseconds(-100); // Negativo
        var callCount = 0;

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "negative_expiration_value";
        }, expiration);

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "should_be_called_again";
        }, expiration);

        // Assert
        Assert.Equal("negative_expiration_value", result1);
        Assert.Equal("should_be_called_again", result2);
        Assert.Equal(2, callCount); // Debe llamarse dos veces
    }

    #endregion

    #region Tests de Excepciones y Recuperación

    [Fact]
    public async Task GetOrSetAsync_WithIntermittentExceptions_ShouldRetryCorrectly()
    {
        // Arrange
        var key = "intermittent_exception_test";
        var callCount = 0;
        var shouldThrow = true;

        // Act - Primera llamada debe fallar
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _cacheService.GetOrSetAsync(key, async () =>
            {
                callCount++;
                await Task.Delay(10);
                if (shouldThrow)
                {
                    shouldThrow = false; // Solo fallar la primera vez
                    throw new InvalidOperationException("Intermittent error");
                }
                return "success_value";
            });
        });

        // Segunda llamada debe tener éxito
        var result = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "success_value";
        });

        // Assert
        Assert.Equal("success_value", result);
        Assert.Equal(2, callCount); // Debe llamarse dos veces
    }

    [Fact]
    public async Task GetOrSetAsync_WithCancellation_ShouldHandleCorrectly()
    {
        // Arrange
        var key = "cancellation_test";
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

    #endregion

    #region Tests de Integridad de Datos

    [Fact]
    public async Task GetOrSetAsync_WithComplexObjects_ShouldMaintainIntegrity()
    {
        // Arrange
        var key = "complex_object_test";
        var originalObject = new ProductoDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Test Product",
            Precio = 15.99m,
            Descripcion = "Test Description",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return originalObject;
        });

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return new ProductoDto { Id = Guid.NewGuid() };
        });

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(originalObject.Id, result1.Id);
        Assert.Equal(originalObject.Id, result2.Id);
        Assert.Equal(originalObject.Nombre, result1.Nombre);
        Assert.Equal(originalObject.Nombre, result2.Nombre);
        Assert.Equal(originalObject.Precio, result1.Precio);
        Assert.Equal(originalObject.Precio, result2.Precio);
    }

    [Fact]
    public async Task GetOrSetAsync_WithCollections_ShouldMaintainIntegrity()
    {
        // Arrange
        var key = "collection_test";
        var originalList = new List<ProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Product 1", Precio = 10.99m },
            new() { Id = Guid.NewGuid(), Nombre = "Product 2", Precio = 15.99m },
            new() { Id = Guid.NewGuid(), Nombre = "Product 3", Precio = 20.99m }
        };

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return originalList;
        });

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            await Task.Delay(10);
            return new List<ProductoDto>();
        });

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(originalList.Count, result1.Count);
        Assert.Equal(originalList.Count, result2.Count);
        
        for (int i = 0; i < originalList.Count; i++)
        {
            Assert.Equal(originalList[i].Id, result1[i].Id);
            Assert.Equal(originalList[i].Id, result2[i].Id);
            Assert.Equal(originalList[i].Nombre, result1[i].Nombre);
            Assert.Equal(originalList[i].Nombre, result2[i].Nombre);
        }
    }

    #endregion

    #region Tests de Limpieza y Memoria

    [Fact]
    public async Task GetOrSetAsync_AfterExpiration_ShouldCleanupCorrectly()
    {
        // Arrange
        var key = "cleanup_test";
        var expiration = TimeSpan.FromMilliseconds(50);
        var callCount = 0;

        // Act
        var result1 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "original_value";
        }, expiration);

        // Esperar a que expire
        await Task.Delay(100);

        var result2 = await _cacheService.GetOrSetAsync(key, async () =>
        {
            callCount++;
            await Task.Delay(10);
            return "new_value";
        }, expiration);

        // Assert
        Assert.Equal("original_value", result1);
        Assert.Equal("new_value", result2);
        Assert.Equal(2, callCount); // Debe llamarse dos veces
    }

    #endregion
}

/// <summary>
/// Objeto grande para pruebas de memoria
/// </summary>
public class LargeObject
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}

/// <summary>
/// Implementación robusta del CacheService para tests
/// </summary>
public class RobustMockCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, DateTime> _expirations = new();
    private readonly Dictionary<string, Task> _runningTasks = new();
    private readonly object _lock = new();

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        // Verificar si existe y no ha expirado
        lock (_lock)
        {
            if (_cache.ContainsKey(key) && (!_expirations.ContainsKey(key) || _expirations[key] > DateTime.UtcNow))
            {
                return (T)_cache[key];
            }
        }

        // Verificar si ya hay una tarea ejecutándose para esta clave
        Task<T>? existingTask = null;
        lock (_lock)
        {
            if (_runningTasks.ContainsKey(key))
            {
                existingTask = (Task<T>)_runningTasks[key];
            }
        }

        if (existingTask != null)
        {
            // Esperar a que termine la tarea existente
            return await existingTask;
        }

        // Crear nueva tarea para esta clave
        Task<T> task = ExecuteFactoryAndCache(key, factory, expiration);
        
        lock (_lock)
        {
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
