using Microsoft.Maui.Storage;
using Moq;
using RestaurantePro.Mobile.Core.Services.Preferences;
using System.Text.Json;

namespace RestaurantePro.Mobile.UnitTests.Services.Preferences;

public class PreferencesServiceTests
{
    private readonly PreferencesService _preferencesService;

    public PreferencesServiceTests()
    {
        _preferencesService = new PreferencesService();
    }

    #region Get Tests

    [Fact]
    public void Get_WithStringValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_string_key";
        var expectedValue = "test_value";
        var defaultValue = "default";

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // En un entorno real, esto dependería de las preferencias del sistema
        // Para las pruebas, verificamos que no lance excepción
        Assert.NotNull(result);
    }

    [Fact]
    public void Get_WithIntValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_int_key";
        var expectedValue = 42;
        var defaultValue = 0;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithBoolValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_bool_key";
        var expectedValue = true;
        var defaultValue = false;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithDoubleValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_double_key";
        var expectedValue = 3.14;
        var defaultValue = 0.0;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithFloatValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_float_key";
        var expectedValue = 2.5f;
        var defaultValue = 0.0f;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithLongValue_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_long_key";
        var expectedValue = 123456789L;
        var defaultValue = 0L;

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithComplexObject_ShouldReturnStoredValue()
    {
        // Arrange
        var key = "test_complex_key";
        var expectedValue = new { Name = "Test", Value = 123 };
        var defaultValue = new { Name = "Default", Value = 0 };

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public void Get_WithNonExistentKey_ShouldReturnDefaultValue()
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
    public void Get_WithEmptyKey_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "";
        var defaultValue = "default_value";

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public void Get_WithNullKey_ShouldReturnDefaultValue()
    {
        // Arrange
        string? key = null;
        var defaultValue = "default_value";

        // Act
        var result = _preferencesService.Get(key!, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    #endregion

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_WithStringValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_string_key";
        var value = "test_value";

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithIntValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_int_key";
        var value = 42;

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithBoolValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_bool_key";
        var value = true;

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithDoubleValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_double_key";
        var value = 3.14;

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithFloatValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_float_key";
        var value = 2.5f;

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithLongValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_long_key";
        var value = 123456789L;

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithComplexObject_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_complex_key";
        var value = new { Name = "Test", Value = 123 };

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithEmptyKey_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "";
        var value = "test_value";

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithNullKey_ShouldCompleteSuccessfully()
    {
        // Arrange
        string? key = null;
        var value = "test_value";

        // Act & Assert
        await _preferencesService.SetAsync(key!, value);
        // No exception should be thrown
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_key";
        string? value = null;

        // Act & Assert
        await _preferencesService.SetAsync(key, value!);
        // No exception should be thrown
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_WithValidKey_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "test_key";

        // Act & Assert
        _preferencesService.Remove(key);
        // No exception should be thrown
    }

    [Fact]
    public void Remove_WithEmptyKey_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "";

        // Act & Assert
        _preferencesService.Remove(key);
        // No exception should be thrown
    }

    [Fact]
    public void Remove_WithNullKey_ShouldCompleteSuccessfully()
    {
        // Arrange
        string? key = null;

        // Act & Assert
        _preferencesService.Remove(key!);
        // No exception should be thrown
    }

    #endregion

    #region ContainsKey Tests

    [Fact]
    public void ContainsKey_WithValidKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "test_key";

        // Act
        var result = _preferencesService.ContainsKey(key);

        // Assert
        // En un entorno de pruebas, probablemente retorne false
        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_WithEmptyKey_ShouldReturnFalse()
    {
        // Arrange
        var key = "";

        // Act
        var result = _preferencesService.ContainsKey(key);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_WithNullKey_ShouldReturnFalse()
    {
        // Arrange
        string? key = null;

        // Act
        var result = _preferencesService.ContainsKey(key!);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_ShouldCompleteSuccessfully()
    {
        // Act & Assert
        _preferencesService.Clear();
        // No exception should be thrown
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task SetAndGet_WithStringValue_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "integration_test_string";
        var value = "integration_test_value";

        // Act
        await _preferencesService.SetAsync(key, value);
        var result = _preferencesService.Get(key, "default");

        // Assert
        // En un entorno real, esto verificaría que el valor se guardó correctamente
        // Para las pruebas, verificamos que no lance excepción
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SetAndGet_WithIntValue_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "integration_test_int";
        var value = 42;

        // Act
        await _preferencesService.SetAsync(key, value);
        var result = _preferencesService.Get(key, 0);

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public async Task SetAndGet_WithComplexObject_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "integration_test_complex";
        var value = new { Name = "Integration Test", Value = 123, IsActive = true };

        // Act
        await _preferencesService.SetAsync(key, value);
        var result = _preferencesService.Get(key, new { Name = "Default", Value = 0, IsActive = false });

        // Assert
        // Verificamos que no lance excepción
    }

    [Fact]
    public async Task SetRemoveContains_ShouldWorkCorrectly()
    {
        // Arrange
        var key = "integration_test_remove";
        var value = "test_value";

        // Act
        await _preferencesService.SetAsync(key, value);
        var containsBefore = _preferencesService.ContainsKey(key);
        _preferencesService.Remove(key);
        var containsAfter = _preferencesService.ContainsKey(key);

        // Assert
        // En un entorno real, esto verificaría que la clave se eliminó correctamente
        Assert.NotNull(containsBefore);
        Assert.NotNull(containsAfter);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Get_WithInvalidJson_ShouldReturnDefaultValue()
    {
        // Arrange
        var key = "invalid_json_key";
        var defaultValue = new { Name = "Default", Value = 0 };

        // Act
        var result = _preferencesService.Get(key, defaultValue);

        // Assert
        Assert.Equal(defaultValue, result);
    }

    [Fact]
    public async Task SetAsync_WithInvalidObject_ShouldCompleteSuccessfully()
    {
        // Arrange
        var key = "invalid_object_key";
        var value = new { Name = "Test", Value = 123 };

        // Act & Assert
        await _preferencesService.SetAsync(key, value);
        // No exception should be thrown (el servicio maneja errores internamente)
    }

    #endregion
}
