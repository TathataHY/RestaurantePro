using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para cambiar el color según el estado de la comanda
/// </summary>
public class EstadoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado.ToLower() switch
            {
                "creada" => Color.FromArgb("#FF6B6B"), // Rojo - Nueva comanda
                "enproceso" => Color.FromArgb("#4ECDC4"), // Verde azulado - En preparación
                "lista" => Color.FromArgb("#45B7D1"), // Azul - Lista para entregar
                "entregada" => Color.FromArgb("#96CEB4"), // Verde - Entregada
                "finalizada" => Color.FromArgb("#FFEAA7"), // Amarillo - Finalizada
                "cancelada" => Color.FromArgb("#DDA0DD"), // Púrpura - Cancelada
                _ => Color.FromArgb("#95A5A6") // Gris - Estado desconocido
            };
        }
        
        return Color.FromArgb("#95A5A6"); // Gris por defecto
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}