using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Configuración global de JSON para todos los tests de integración
/// </summary>
public static class GlobalJsonConfiguration
{
    static GlobalJsonConfiguration()
    {
        // Configurar opciones JSON por defecto globalmente
        JsonSerializerOptions.Default.PropertyNameCaseInsensitive = true;
        JsonSerializerOptions.Default.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Obtiene las opciones JSON configuradas para tests
    /// </summary>
    public static JsonSerializerOptions GetTestOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
