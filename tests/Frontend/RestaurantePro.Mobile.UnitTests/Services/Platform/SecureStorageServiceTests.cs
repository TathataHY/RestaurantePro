using RestaurantePro.Mobile.Core.Services.Platform;

namespace RestaurantePro.Mobile.UnitTests.Services.Platform;

/// <summary>
/// Implementación mock del servicio de almacenamiento seguro para pruebas
/// </summary>
public class MockSecureStorageService : ISecureStorageService
{
    private readonly Dictionary<string, string> _storage = new();

    public async Task SetAsync(string key, string value)
    {
        if (key != null)
            _storage[key] = value;
        await Task.CompletedTask;
    }

    public async Task<string?> GetAsync(string key)
    {
        if (key == null)
            return null;
            
        _storage.TryGetValue(key, out var value);
        await Task.CompletedTask;
        return value;
    }

    public async Task RemoveAsync(string key)
    {
        if (key != null)
            _storage.Remove(key);
        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        _storage.Clear();
        await Task.CompletedTask;
    }

    public int Count => _storage.Count;
    public bool ContainsKey(string key) => _storage.ContainsKey(key);
    public IEnumerable<string> Keys => _storage.Keys;
    public IEnumerable<string> Values => _storage.Values;
}

public class SecureStorageServiceTests
{
    private readonly MockSecureStorageService _secureStorageService;

    public SecureStorageServiceTests()
    {
        _secureStorageService = new MockSecureStorageService();
    }

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_WithValidKeyAndValue_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ShouldStoreValue()
    {
        // Arrange
        var key = "";
        var value = "test-value";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithEmptyValue_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        string? value = null;

        // Act
        await _secureStorageService.SetAsync(key, value!);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithSpecialCharacters_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key-!@#$%^&*()";
        var value = "test-value-!@#$%^&*()";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithUnicodeCharacters_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key-🚀";
        var value = "test-value-🌟";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithLongValue_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = new string('A', 10000); // 10000 caracteres

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithExistingKey_ShouldOverwriteValue()
    {
        // Arrange
        var key = "test-key";
        var originalValue = "original-value";
        var newValue = "new-value";

        await _secureStorageService.SetAsync(key, originalValue);

        // Act
        await _secureStorageService.SetAsync(key, newValue);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(newValue, await _secureStorageService.GetAsync(key));
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        await _secureStorageService.SetAsync(key, value);

        // Act
        var result = await _secureStorageService.GetAsync(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existing-key";

        // Act
        var result = await _secureStorageService.GetAsync(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithEmptyKey_ShouldReturnValue()
    {
        // Arrange
        var key = "";
        var value = "test-value";
        await _secureStorageService.SetAsync(key, value);

        // Act
        var result = await _secureStorageService.GetAsync(key);

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public async Task GetAsync_WithNullKey_ShouldReturnNull()
    {
        // Arrange
        string? key = null;

        // Act
        var result = await _secureStorageService.GetAsync(key!);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithExistingKey_ShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        await _secureStorageService.SetAsync(key, value);
        Assert.True(_secureStorageService.ContainsKey(key));

        // Act
        await _secureStorageService.RemoveAsync(key);

        // Assert
        Assert.False(_secureStorageService.ContainsKey(key));
        Assert.Null(await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistingKey_ShouldNotThrow()
    {
        // Arrange
        var key = "non-existing-key";

        // Act & Assert
        await _secureStorageService.RemoveAsync(key);
        // No debería lanzar excepción
    }

    [Fact]
    public async Task RemoveAsync_WithEmptyKey_ShouldRemoveValue()
    {
        // Arrange
        var key = "";
        var value = "test-value";
        await _secureStorageService.SetAsync(key, value);
        Assert.True(_secureStorageService.ContainsKey(key));

        // Act
        await _secureStorageService.RemoveAsync(key);

        // Assert
        Assert.False(_secureStorageService.ContainsKey(key));
        Assert.Null(await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task RemoveAsync_WithNullKey_ShouldNotThrow()
    {
        // Arrange
        string? key = null;

        // Act & Assert
        await _secureStorageService.RemoveAsync(key!);
        // No debería lanzar excepción
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_WithEmptyStorage_ShouldNotThrow()
    {
        // Act & Assert
        await _secureStorageService.ClearAsync();
        // No debería lanzar excepción
    }

    [Fact]
    public async Task ClearAsync_WithMultipleItems_ShouldRemoveAllItems()
    {
        // Arrange
        await _secureStorageService.SetAsync("key1", "value1");
        await _secureStorageService.SetAsync("key2", "value2");
        await _secureStorageService.SetAsync("key3", "value3");
        Assert.Equal(3, _secureStorageService.Count);

        // Act
        await _secureStorageService.ClearAsync();

        // Assert
        Assert.Equal(0, _secureStorageService.Count);
        Assert.Null(await _secureStorageService.GetAsync("key1"));
        Assert.Null(await _secureStorageService.GetAsync("key2"));
        Assert.Null(await _secureStorageService.GetAsync("key3"));
    }

    [Fact]
    public async Task ClearAsync_WithSingleItem_ShouldRemoveItem()
    {
        // Arrange
        await _secureStorageService.SetAsync("key1", "value1");
        Assert.Equal(1, _secureStorageService.Count);

        // Act
        await _secureStorageService.ClearAsync();

        // Assert
        Assert.Equal(0, _secureStorageService.Count);
        Assert.Null(await _secureStorageService.GetAsync("key1"));
    }

    #endregion

    #region Multiple Operations Tests

    [Fact]
    public async Task MultipleOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var key1 = "key1";
        var key2 = "key2";
        var value1 = "value1";
        var value2 = "value2";

        // Act & Assert
        // Set multiple values
        await _secureStorageService.SetAsync(key1, value1);
        await _secureStorageService.SetAsync(key2, value2);
        Assert.Equal(2, _secureStorageService.Count);

        // Get values
        Assert.Equal(value1, await _secureStorageService.GetAsync(key1));
        Assert.Equal(value2, await _secureStorageService.GetAsync(key2));

        // Remove one value
        await _secureStorageService.RemoveAsync(key1);
        Assert.Equal(1, _secureStorageService.Count);
        Assert.Null(await _secureStorageService.GetAsync(key1));
        Assert.Equal(value2, await _secureStorageService.GetAsync(key2));

        // Clear all
        await _secureStorageService.ClearAsync();
        Assert.Equal(0, _secureStorageService.Count);
        Assert.Null(await _secureStorageService.GetAsync(key2));
    }

    [Fact]
    public async Task OverwriteValue_ShouldUpdateCorrectly()
    {
        // Arrange
        var key = "test-key";
        var originalValue = "original-value";
        var newValue = "new-value";

        // Act
        await _secureStorageService.SetAsync(key, originalValue);
        Assert.Equal(originalValue, await _secureStorageService.GetAsync(key));

        await _secureStorageService.SetAsync(key, newValue);
        Assert.Equal(newValue, await _secureStorageService.GetAsync(key));

        // Assert
        Assert.Equal(1, _secureStorageService.Count);
        Assert.True(_secureStorageService.ContainsKey(key));
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task SetAsync_WithVeryLongKey_ShouldStoreValue()
    {
        // Arrange
        var key = new string('A', 1000); // 1000 caracteres
        var value = "test-value";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithVeryLongValue_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = new string('A', 100000); // 100000 caracteres

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithJsonValue_ShouldStoreValue()
    {
        // Arrange
        var key = "json-key";
        var value = """{"name": "test", "value": 123, "active": true}""";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithXmlValue_ShouldStoreValue()
    {
        // Arrange
        var key = "xml-key";
        var value = """<root><item>test</item></root>""";

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    [Fact]
    public async Task SetAsync_WithBase64Value_ShouldStoreValue()
    {
        // Arrange
        var key = "base64-key";
        var value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-value"));

        // Act
        await _secureStorageService.SetAsync(key, value);

        // Assert
        Assert.True(_secureStorageService.ContainsKey(key));
        Assert.Equal(value, await _secureStorageService.GetAsync(key));
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentSetOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var tasks = new List<Task>();
        var keys = new[] { "key1", "key2", "key3", "key4", "key5" };
        var values = new[] { "value1", "value2", "value3", "value4", "value5" };

        // Act
        for (int i = 0; i < keys.Length; i++)
        {
            int index = i; // Capturar el índice para evitar closure
            tasks.Add(_secureStorageService.SetAsync(keys[index], values[index]));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, _secureStorageService.Count);
        for (int i = 0; i < keys.Length; i++)
        {
            Assert.True(_secureStorageService.ContainsKey(keys[i]));
            Assert.Equal(values[i], await _secureStorageService.GetAsync(keys[i]));
        }
    }

    [Fact]
    public async Task ConcurrentGetOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        await _secureStorageService.SetAsync(key, value);

        var tasks = new List<Task<string?>>();
        var numberOfGets = 10;

        // Act
        for (int i = 0; i < numberOfGets; i++)
        {
            tasks.Add(_secureStorageService.GetAsync(key));
        }
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(numberOfGets, results.Length);
        foreach (var result in results)
        {
            Assert.Equal(value, result);
        }
    }

    [Fact]
    public async Task ConcurrentRemoveOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3", "key4", "key5" };
        var values = new[] { "value1", "value2", "value3", "value4", "value5" };

        // Set all values
        for (int i = 0; i < keys.Length; i++)
        {
            await _secureStorageService.SetAsync(keys[i], values[i]);
        }
        Assert.Equal(5, _secureStorageService.Count);

        // Act
        var tasks = new List<Task>();
        foreach (var key in keys)
        {
            tasks.Add(_secureStorageService.RemoveAsync(key));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(0, _secureStorageService.Count);
        foreach (var key in keys)
        {
            Assert.False(_secureStorageService.ContainsKey(key));
            Assert.Null(await _secureStorageService.GetAsync(key));
        }
    }

    #endregion
}
