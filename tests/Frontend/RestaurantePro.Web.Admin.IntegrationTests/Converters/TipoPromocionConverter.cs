using System.Text.Json;
using System.Text.Json.Serialization;
using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Web.Admin.IntegrationTests.Converters;

/// <summary>
/// Convertidor JSON para el enum TipoPromocion
/// </summary>
public class TipoPromocionConverter : JsonConverter<RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion>
{
    public override RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse<RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion>(stringValue, true, out var result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion), intValue))
            {
                return (RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion)intValue;
            }
        }

        throw new JsonException($"No se pudo convertir '{reader.GetString()}' a TipoPromocion");
    }

    public override void Write(Utf8JsonWriter writer, RestaurantePro.Domain.Comercial.Promociones.Enums.TipoPromocion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
