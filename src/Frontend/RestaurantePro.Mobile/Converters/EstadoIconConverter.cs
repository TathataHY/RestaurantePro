using System.Globalization;

namespace RestaurantePro.Mobile.Converters;

/// <summary>
/// Convertidor para obtener el icono basado en el estado (mesas o comandas)
/// </summary>
public class EstadoIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string estado)
            return "❓";

        return estado.ToLower() switch
        {
            // Estados de Mesas
            "disponible" => "✅",        // Check verde
            "ocupada" => "👥",           // Personas
            "reservada" => "📅",         // Calendario
            "mantenimiento" => "🔧",     // Herramienta
            "fuera de servicio" => "⚠️", // Advertencia
            
            // Estados de Comandas
            "pendiente" => "⏳",         // Reloj de arena - esperando
            "en_preparacion" => "👨‍🍳",    // Chef - en cocina
            "preparando" => "👨‍🍳",       // Chef - alias de en_preparacion
            "lista" => "🔔",            // Campana - lista para servir
            "servida" => "🍽️",          // Plato servido
            "finalizada" => "✅",        // Check - completada
            "cancelada" => "❌",         // X roja - cancelada
            "completada" => "✅",        // Check - completada
            
            _ => "❓"                    // Interrogación por defecto
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 