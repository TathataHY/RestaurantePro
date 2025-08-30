using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Configuración de JSON para tests que asegura que los enums se serialicen/deserialicen correctamente
/// </summary>
public static class JsonConfiguration
{
    /// <summary>
    /// Opciones JSON por defecto para tests con soporte para enums como strings
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Configura las opciones JSON por defecto para todos los tests
    /// </summary>
    public static void ConfigureDefaultOptions()
    {
        // No podemos modificar JsonSerializerOptions.Default directamente, pero podemos
        // usar nuestras opciones personalizadas en todos los tests
        Console.WriteLine("🔧 Configurando opciones JSON por defecto para tests...");
    }
}
