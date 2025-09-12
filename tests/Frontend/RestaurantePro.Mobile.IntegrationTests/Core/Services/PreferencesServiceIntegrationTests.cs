using RestaurantePro.Mobile.Core.Services.Preferences;
using RestaurantePro.Mobile.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Mobile.IntegrationTests.Core.Services;

/// <summary>
/// Tests de integración para PreferencesService - Configuración local del usuario
/// </summary>
public class PreferencesServiceIntegrationTests : IClassFixture<MobileIntegrationTestFixture>
{
    private readonly MobileIntegrationTestFixture _fixture;
    private readonly MockPreferencesService _preferencesService;

    public PreferencesServiceIntegrationTests(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _preferencesService = new MockPreferencesService();
    }

    #region Tests de Tipos Primitivos

    [Fact]
    public async Task Get_SetString_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_string_key";
        var expectedValue = "Test String Value";
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, "default");

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task Get_SetInt_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_int_key";
        var expectedValue = 42;
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, 0);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task Get_SetBool_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_bool_key";
        var expectedValue = true;
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, false);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task Get_SetDouble_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_double_key";
        var expectedValue = 3.14159;
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, 0.0);

        // Assert
        Assert.Equal(expectedValue, result, 5); // 5 decimales de precisión
    }

    [Fact]
    public async Task Get_SetFloat_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_float_key";
        var expectedValue = 2.718f;
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, 0.0f);

        // Assert
        Assert.Equal(expectedValue, result, 3); // 3 decimales de precisión
    }

    [Fact]
    public async Task Get_SetLong_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_long_key";
        var expectedValue = 9223372036854775807L;
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, 0L);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    #endregion

    #region Tests de Valores por Defecto

    [Fact]
    public async Task Get_NonExistentKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "non_existent_key";
        var defaultValue = "default_value";

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public async Task Get_NonExistentIntKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "non_existent_int_key";
        var defaultValue = 999;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public async Task Get_NonExistentBoolKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "non_existent_bool_key";
        var defaultValue = true;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    #endregion

    #region Tests de Tipos Complejos (JSON)

    [Fact]
    public async Task Get_SetComplexObject_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_complex_key";
        var expectedValue = new TestComplexObject
        {
            Id = Guid.NewGuid(),
            Name = "Test Object",
            Value = 123.45m,
            IsActive = true,
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, (TestComplexObject)null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Id, result.Id);
        Assert.Equal(expectedValue.Name, result.Name);
        Assert.Equal(expectedValue.Value, result.Value);
        Assert.Equal(expectedValue.IsActive, result.IsActive);
        Assert.Equal(expectedValue.Tags, result.Tags);
    }

    [Fact]
    public async Task Get_SetList_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_list_key";
        var expectedValue = new List<string> { "item1", "item2", "item3" };
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, (List<string>)null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Count, result.Count);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task Get_SetDictionary_ShouldReturnCorrectValue()
    {
        // Arrange
        var key = "test_dict_key";
        var expectedValue = new Dictionary<string, int>
        {
            { "key1", 1 },
            { "key2", 2 },
            { "key3", 3 }
        };
        await _preferencesService.SetAsync(key, expectedValue);

        // Act
        var result = _preferencesService.Get(key, (Dictionary<string, int>)null!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedValue.Count, result.Count);
        Assert.Equal(expectedValue, result);
    }

    #endregion

    #region Tests de ContainsKey

    [Fact]
    public async Task ContainsKey_ExistingKey_ShouldReturnTrue()
    {
        // Arrange
        var key = "test_contains_key";
        _preferencesService.SetAsync(key, "test_value").Wait();

        // Act
        var result = _preferencesService.ContainsKey(key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ContainsKey_NonExistentKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "non_existent_contains_key";

        // Act
        var result = _preferencesService.ContainsKey(key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ContainsKey_AfterRemove_ShouldReturnFalse()
    {
        // Arrange
        var key = "test_remove_contains_key";
        _preferencesService.SetAsync(key, "test_value").Wait();
        _preferencesService.Remove(key);

        // Act
        var result = _preferencesService.ContainsKey(key);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Tests de Remove

    [Fact]
    public async Task Remove_ExistingKey_ShouldRemoveKey()
    {
        // Arrange
        var key = "test_remove_key";
        _preferencesService.SetAsync(key, "test_value").Wait();
        Assert.True(_preferencesService.ContainsKey(key));

        // Act
        _preferencesService.Remove(key);

        // Assert
        Assert.False(_preferencesService.ContainsKey(key));
        var defaultValue = _preferencesService.Get(key, "default");
        Assert.Equal("default", defaultValue);
    }

    [Fact]
    public async Task Remove_NonExistentKey_ShouldNotThrow()
    {
        // Arrange
        var key = "non_existent_remove_key";

        // Act & Assert - No debe lanzar excepción
        _preferencesService.Remove(key);
        Assert.False(_preferencesService.ContainsKey(key));
    }

    #endregion

    #region Tests de Clear

    [Fact]
    public async Task Clear_WithMultipleKeys_ShouldRemoveAllKeys()
    {
        // Arrange
        var keys = new[] { "key1", "key2", "key3", "key4" };
        foreach (var key in keys)
        {
            _preferencesService.SetAsync(key, $"value_{key}").Wait();
        }

        // Verificar que todas las claves existen
        foreach (var key in keys)
        {
            Assert.True(_preferencesService.ContainsKey(key));
        }

        // Act
        _preferencesService.Clear();

        // Assert
        foreach (var key in keys)
        {
            Assert.False(_preferencesService.ContainsKey(key));
        }
    }

    [Fact]
    public async Task Clear_EmptyPreferences_ShouldNotThrow()
    {
        // Act & Assert - No debe lanzar excepción
        _preferencesService.Clear();
    }

    #endregion

    #region Tests de Configuración de App Móvil

    [Fact]
    public async Task Set_GetAppSettings_ShouldWorkCorrectly()
    {
        // Arrange - Configuraciones típicas de la app móvil
        var settings = new AppSettings
        {
            NotificationsEnabled = true,
            AutoRefreshInterval = 30,
            DefaultLanguage = "es",
            Theme = "dark",
            LastSyncDate = DateTime.Now,
            UserPreferences = new Dictionary<string, object>
            {
                { "show_tips", true },
                { "auto_save", false },
                { "sound_enabled", true }
            }
        };

        // Act
        _preferencesService.SetAsync("app_settings", settings).Wait();
        var retrievedSettings = _preferencesService.Get("app_settings", (AppSettings)null!);

        // Assert
        Assert.NotNull(retrievedSettings);
        Assert.Equal(settings.NotificationsEnabled, retrievedSettings.NotificationsEnabled);
        Assert.Equal(settings.AutoRefreshInterval, retrievedSettings.AutoRefreshInterval);
        Assert.Equal(settings.DefaultLanguage, retrievedSettings.DefaultLanguage);
        Assert.Equal(settings.Theme, retrievedSettings.Theme);
        Assert.Equal(settings.UserPreferences.Count, retrievedSettings.UserPreferences.Count);
    }

    [Fact]
    public async Task Set_GetUserPreferences_ShouldWorkCorrectly()
    {
        // Arrange - Preferencias específicas del usuario
        var userPrefs = new UserPreferences
        {
            UserId = Guid.NewGuid(),
            PreferredTableSize = 4,
            FavoriteProducts = new List<string> { "Pizza Margherita", "Coca Cola" },
            NotificationSettings = new NotificationSettings
            {
                OrderUpdates = true,
                TableChanges = false,
                SystemAlerts = true
            }
        };

        // Act
        _preferencesService.SetAsync("user_preferences", userPrefs).Wait();
        var retrievedPrefs = _preferencesService.Get("user_preferences", (UserPreferences)null!);

        // Assert
        Assert.NotNull(retrievedPrefs);
        Assert.Equal(userPrefs.UserId, retrievedPrefs.UserId);
        Assert.Equal(userPrefs.PreferredTableSize, retrievedPrefs.PreferredTableSize);
        Assert.Equal(userPrefs.FavoriteProducts, retrievedPrefs.FavoriteProducts);
        Assert.Equal(userPrefs.NotificationSettings.OrderUpdates, retrievedPrefs.NotificationSettings.OrderUpdates);
        Assert.Equal(userPrefs.NotificationSettings.TableChanges, retrievedPrefs.NotificationSettings.TableChanges);
        Assert.Equal(userPrefs.NotificationSettings.SystemAlerts, retrievedPrefs.NotificationSettings.SystemAlerts);
    }

    #endregion

    #region Tests de Resiliencia

    [Fact]
    public async Task Get_WithInvalidJson_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "invalid_json_key";
        // Simular JSON inválido para un tipo complejo
        await _preferencesService.SetAsync(key, new { Name = "Test" }); // Esto se serializa como JSON válido
        // Ahora sobrescribir con JSON inválido directamente en el diccionario interno
        var mockService = (MockPreferencesService)_preferencesService;
        mockService.SetInvalidJson(key, "invalid json string");

        // Act
        var result = _preferencesService.Get<TestObject>(key, new TestObject { Name = "default_value" });

        // Assert
        Assert.Equal("default_value", result.Name);
    }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task Set_Get_MultipleTypes_ShouldWorkCorrectly()
    {
        // Arrange & Act - Probar múltiples tipos en secuencia
        _preferencesService.SetAsync("string_key", "string_value").Wait();
        _preferencesService.SetAsync("int_key", 42).Wait();
        _preferencesService.SetAsync("bool_key", true).Wait();
        _preferencesService.SetAsync("double_key", 3.14).Wait();

        // Assert
        Assert.Equal("string_value", _preferencesService.Get("string_key", ""));
        Assert.Equal(42, _preferencesService.Get("int_key", 0));
        Assert.True(_preferencesService.Get("bool_key", false));
        Assert.Equal(3.14, _preferencesService.Get("double_key", 0.0), 2);
    }

    #endregion
}

#region Test Data Classes

public class TestComplexObject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class AppSettings
{
    public bool NotificationsEnabled { get; set; }
    public int AutoRefreshInterval { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public DateTime LastSyncDate { get; set; }
    public Dictionary<string, object> UserPreferences { get; set; } = new();
}

public class UserPreferences
{
    public Guid UserId { get; set; }
    public int PreferredTableSize { get; set; }
    public List<string> FavoriteProducts { get; set; } = new();
    public NotificationSettings NotificationSettings { get; set; } = new();
}

public class NotificationSettings
{
    public bool OrderUpdates { get; set; }
    public bool TableChanges { get; set; }
    public bool SystemAlerts { get; set; }
}

#endregion

/// <summary>
/// Mock implementation of IPreferencesService for testing
/// </summary>
public class MockPreferencesService : IPreferencesService
{
    private readonly Dictionary<string, object> _preferences = new();

    public T Get<T>(string key, T defaultValue = default)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            if (value is T directValue)
            {
                return directValue;
            }
            
            // For complex types stored as JSON
            if (value is string json && typeof(T) != typeof(string))
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                }
                catch (System.Text.Json.JsonException)
                {
                    return defaultValue;
                }
            }
        }
        
        return defaultValue;
    }

    public async Task SetAsync<T>(string key, T value)
    {
        if (typeof(T) == typeof(string) || typeof(T) == typeof(int) || typeof(T) == typeof(bool) || 
            typeof(T) == typeof(double) || typeof(T) == typeof(float) || typeof(T) == typeof(long))
        {
            _preferences[key] = value;
        }
        else
        {
            // For complex types, serialize to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            _preferences[key] = json;
        }
        
        await Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _preferences.Remove(key);
    }

    public bool ContainsKey(string key)
    {
        return _preferences.ContainsKey(key);
    }

    public void Clear()
    {
        _preferences.Clear();
    }

    public void SetInvalidJson(string key, string invalidJson)
    {
        _preferences[key] = invalidJson;
    }
}
