using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para mostrar texto según el valor booleano
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var flag = value is bool b && b;
            return flag ? "Activa" : "Inactiva";
        }
        catch
        {
            return "Inactiva";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
