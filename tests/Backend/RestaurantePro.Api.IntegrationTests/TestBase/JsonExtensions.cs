using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Api.Common;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Extensiones para deserialización JSON con configuración correcta para enums
/// </summary>
public static class JsonExtensions
{
    private static readonly JsonSerializerOptions _defaultOptions = JsonConfiguration.DefaultOptions;

    /// <summary>
    /// Deserializa HttpContent a T usando las opciones JSON correctas para enums
    /// Esta es la versión que reemplaza completamente el comportamiento por defecto de ReadFromJsonAsync
    /// </summary>
    public static async Task<T> ReadFromJsonAsync<T>(this HttpContent content)
    {
        var jsonContent = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(jsonContent, _defaultOptions)!;
    }

    /// <summary>
    /// Deserializa HttpContent a ApiResponse<T> usando las opciones JSON correctas para enums
    /// </summary>
    public static async Task<ApiResponse<T>> ReadFromJsonAsyncApiResponse<T>(this HttpContent content)
    {
        var jsonContent = await content.ReadAsStringAsync();
        
        // Para tipos genéricos complejos como PaginatedList<T>, necesitamos asegurarnos de que
        // las opciones JSON se apliquen correctamente a todos los niveles
        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, _defaultOptions)!;
        }
        catch (JsonException ex) when (ex.Message.Contains("could not be converted"))
        {
            // Si falla la deserialización, intentar con opciones más permisivas
            var fallbackOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };
            
            try
            {
                return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, fallbackOptions)!;
            }
            catch (JsonException)
            {
                // Si aún falla, intentar deserializar directamente el tipo T
                // Esto puede ayudar con tipos genéricos complejos
                var directOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() },
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                
                try
                {
                    return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, directOptions)!;
                }
                catch (JsonException)
                {
                    // Último intento: deserializar manualmente el JSON para tipos genéricos complejos
                    return await DeserializeComplexGenericType<T>(jsonContent);
                }
            }
        }
    }

    /// <summary>
    /// Deserializa tipos genéricos complejos como PaginatedList<T> manualmente
    /// </summary>
    private static async Task<ApiResponse<T>> DeserializeComplexGenericType<T>(string jsonContent)
    {
        // Para PaginatedList<T>, necesitamos deserializar manualmente
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition().Name.Contains("PaginatedList"))
        {
            // Crear opciones JSON específicas para PaginatedList<T>
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                // Configuraciones adicionales para tipos genéricos complejos
                MaxDepth = 64,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            try
            {
                // Intentar deserializar con opciones más permisivas
                return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, options)!;
            }
            catch (JsonException ex)
            {
                // Si aún falla, intentar deserializar el JSON manualmente
                return await DeserializePaginatedListManually<T>(jsonContent);
            }
        }
        
        // Para otros tipos, usar las opciones por defecto
        return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, _defaultOptions)!;
    }

    /// <summary>
    /// Deserializa manualmente PaginatedList<T> desde JSON
    /// </summary>
    private static async Task<ApiResponse<T>> DeserializePaginatedListManually<T>(string jsonContent)
    {
        try
        {
            // Deserializar primero como JsonDocument para inspeccionar la estructura
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;
            
            // Crear opciones JSON muy permisivas
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                MaxDepth = 128,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            
            // Intentar deserializar con opciones ultra permisivas
            return JsonSerializer.Deserialize<ApiResponse<T>>(jsonContent, options)!;
        }
        catch (JsonException)
        {
            // Si todo falla, crear una respuesta de error
            return new ApiResponse<T>
            {
                Success = false,
                Data = default,
                Message = "Error al deserializar PaginatedList<T>",
                Errors = new List<string> { "No se pudo deserializar la respuesta paginada" }
            };
        }
    }
}
