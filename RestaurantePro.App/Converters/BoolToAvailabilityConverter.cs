using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace RestaurantePro.App.Converters
{
    public class BoolToAvailabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Disponible" : "No Disponible";
            }
            return "No Disponible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string availability)
            {
                return availability == "Disponible";
            }
            return false;
        }
    }
}