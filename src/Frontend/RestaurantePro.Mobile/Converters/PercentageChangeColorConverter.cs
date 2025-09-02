using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para mostrar colores según el porcentaje de cambio (positivo/negativo)
/// </summary>
public class PercentageChangeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal percentage)
        {
            return percentage >= 0 ? Colors.Green : Colors.Red;
        }
        
        if (value is double doublePercentage)
        {
            return doublePercentage >= 0 ? Colors.Green : Colors.Red;
        }
        
        if (value is int intPercentage)
        {
            return intPercentage >= 0 ? Colors.Green : Colors.Red;
        }

        return Colors.Gray; // Gris por defecto
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
