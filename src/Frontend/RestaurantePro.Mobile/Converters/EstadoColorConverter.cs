using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para obtener el color basado en el estado de la mesa
/// </summary>
public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string estado)
            return Colors.Gray;

        return estado.ToLower() switch
        {
            "disponible" => Colors.Green,      // Verde
            "ocupada" => Colors.Red,           // Rojo
            "reservada" => Colors.Orange,      // Naranja
            "mantenimiento" => Colors.Gray,    // Gris
            "fuera de servicio" => Colors.DarkGray, // Gris oscuro
            _ => Colors.Blue                   // Azul por defecto
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 