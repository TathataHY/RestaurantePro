using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var flag = value is bool b && b;
            // true => WarningColor (amarillo); false => TextSecondary (gris)
            return flag
                ? Application.Current?.Resources["WarningColor"] as Color ?? Colors.Orange
                : Application.Current?.Resources["TextSecondary"] as Color ?? Colors.Gray;
        }
        catch
        {
            return Application.Current?.Resources["TextSecondary"] as Color ?? Colors.Gray;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


