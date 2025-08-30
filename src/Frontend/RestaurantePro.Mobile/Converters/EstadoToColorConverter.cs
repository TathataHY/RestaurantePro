using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para mostrar colores según el estado de la reservación
/// </summary>
public class EstadoToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado.ToLowerInvariant() switch
            {
                "pendiente" => Colors.Orange,      // Naranja
                "confirmada" => Colors.Green,      // Verde
                "cancelada" => Colors.Red,         // Rojo
                "completada" => Colors.Blue,       // Azul
                "reprogramada" => Colors.Purple,   // Púrpura
                _ => Colors.Gray                   // Gris por defecto
            };
        }

        return Colors.Gray; // Gris por defecto
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 