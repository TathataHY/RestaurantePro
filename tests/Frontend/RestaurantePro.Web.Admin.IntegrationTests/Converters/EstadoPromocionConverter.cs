using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Web.Admin.IntegrationTests.Converters;

/// <summary>
/// Convertidor JSON para el enum EstadoPromocion
/// </summary>
public class EstadoPromocionConverter : JsonConverter<RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion>
{
    public override RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse<RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion>(stringValue, true, out var result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion), intValue))
            {
                return (RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion)intValue;
            }
        }

        throw new JsonException($"No se pudo convertir '{reader.GetString()}' a EstadoPromocion");
    }

    public override void Write(Utf8JsonWriter writer, RestaurantePro.Domain.Comercial.Promociones.Enums.EstadoPromocion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
