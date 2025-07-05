using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para obtener el icono basado en el estado de la mesa
/// </summary>
public class EstadoIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string estado)
            return "❓";

        return estado.ToLower() switch
        {
            "disponible" => "✅",
            "ocupada" => "👥",
            "reservada" => "📅",
            "mantenimiento" => "🔧",
            "fuera de servicio" => "⚠️",
            _ => "❓"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 