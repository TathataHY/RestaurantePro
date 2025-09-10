using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para convertir string de color hexadecimal a Color
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is string colorString && !string.IsNullOrWhiteSpace(colorString))
            {
                return Color.FromArgb(colorString);
            }
            
            return Colors.Gray; // Color por defecto
        }
        catch
        {
            return Colors.Gray; // Color por defecto en caso de error
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
