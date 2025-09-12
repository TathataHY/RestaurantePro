using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Domain.Core.Usuarios.Enums;

namespace RestaurantePro.Web.Admin.IntegrationTests.Converters;

public class EstadoUsuarioConverter : JsonConverter<EstadoUsuario>
{
    public override EstadoUsuario Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        string? value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Value cannot be null or empty");
        }

        // Try to parse the enum value
        if (Enum.TryParse<EstadoUsuario>(value, true, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to convert '{value}' to {nameof(EstadoUsuario)}");
    }

    public override void Write(Utf8JsonWriter writer, EstadoUsuario value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
