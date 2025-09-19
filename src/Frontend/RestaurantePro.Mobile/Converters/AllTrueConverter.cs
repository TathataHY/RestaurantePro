using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Converter que devuelve true solo si TODOS los valores booleanos de entrada son true.
/// Útil para combinar múltiples condiciones (ej: autorización Y estado de comanda).
/// </summary>
public class AllTrueConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length == 0)
            return false;

        // Verificar que todos los valores sean true
        foreach (var value in values)
        {
            // Convertir a bool si es posible
            if (value is bool boolValue)
            {
                if (!boolValue) return false;
            }
            else if (value != null)
            {
                // Intentar convertir otros tipos a bool
                if (bool.TryParse(value.ToString(), out bool parsed))
                {
                    if (!parsed) return false;
                }
                else
                {
                    // Si no se puede convertir, considerar como false
                    return false;
                }
            }
            else
            {
                // Valor null se considera false
                return false;
            }
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack no está implementado para AllTrueConverter");
    }
}
