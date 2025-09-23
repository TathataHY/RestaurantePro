namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// Representa un elemento de comanda para el dashboard
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderTime { get; set; }
    public List<string> Items { get; set; } = new List<string>();
    
    /// <summary>
    /// Número de mesa formateado para mostrar
    /// </summary>
    public string MesaNumeroDisplay => !string.IsNullOrEmpty(TableNumber) ? TableNumber : "Sin asignar";
    
    /// <summary>
    /// Tiempo transcurrido formateado para mostrar
    /// </summary>
    public string TiempoTranscurrido => TimeAgo;
    
    /// <summary>
    /// Cantidad de productos en la comanda
    /// </summary>
    public int ProductosCount => Items?.Count ?? 0;
    /// <summary>
    /// Tiempo transcurrido desde que se hizo la comanda
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - OrderTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Hace un momento";
            else if (timeSpan.TotalMinutes < 60)
                return $"Hace {timeSpan.Minutes} min";
            else if (timeSpan.TotalHours < 24)
                return $"Hace {timeSpan.Hours}h {timeSpan.Minutes}m";
            else
                return $"Hace {timeSpan.Days} días";
        }
    }
    
    /// <summary>
    /// Color del estado para la UI
    /// </summary>
    public Microsoft.Maui.Graphics.Color StatusColor
    {
        get
        {
            return Status.ToLower() switch
            {
                "pendiente" => Microsoft.Maui.Graphics.Color.FromArgb("#FF6B35"),
                "en progreso" => Microsoft.Maui.Graphics.Color.FromArgb("#4ECDC4"),
                "completada" => Microsoft.Maui.Graphics.Color.FromArgb("#45B7D1"),
                "cancelada" => Microsoft.Maui.Graphics.Color.FromArgb("#FF4757"),
                _ => Microsoft.Maui.Graphics.Color.FromArgb("#95A5A6")
            };
        }
    }
}
