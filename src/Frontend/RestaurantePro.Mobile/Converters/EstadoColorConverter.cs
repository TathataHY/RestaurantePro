using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para obtener el color basado en el estado (mesas o comandas)
/// </summary>
public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string estado)
            return Colors.Gray;

        return estado.ToLower() switch
        {
            // Estados de Mesas
            "disponible" => Colors.Green,          // Verde
            "ocupada" => Colors.Red,               // Rojo
            "reservada" => Colors.Orange,          // Naranja
            "mantenimiento" => Colors.Gray,        // Gris
            "fuera de servicio" => Colors.DarkGray, // Gris oscuro
            
            // Estados de Comandas
            "pendiente" => Colors.Yellow,          // Amarillo - esperando atención
            "en_preparacion" => Colors.Orange,     // Naranja - en cocina
            "preparando" => Colors.Orange,         // Naranja - alias de en_preparacion
            "lista" => Colors.LightGreen,          // Verde claro - lista para servir
            "servida" => Colors.Green,             // Verde - entregada al cliente
            "finalizada" => Colors.Blue,           // Azul - comanda cerrada
            "cancelada" => Colors.Red,             // Rojo - cancelada
            "completada" => Colors.Green,          // Verde - completada
            
            _ => Colors.Blue                       // Azul por defecto
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 