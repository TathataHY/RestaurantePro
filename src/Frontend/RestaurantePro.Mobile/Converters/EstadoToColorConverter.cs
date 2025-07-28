using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para mostrar colores según el estado de la reservación
/// </summary>
public class EstadoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado.ToLowerInvariant() switch
            {
                "pendiente" => Color.FromHex("#FF9800"),    // Naranja
                "confirmada" => Color.FromHex("#4CAF50"),   // Verde
                "cancelada" => Color.FromHex("#F44336"),    // Rojo
                "completada" => Color.FromHex("#2196F3"),   // Azul
                "reprogramada" => Color.FromHex("#9C27B0"), // Púrpura
                _ => Color.FromHex("#9E9E9E")               // Gris por defecto
            };
        }

        return Color.FromHex("#9E9E9E"); // Gris por defecto
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 