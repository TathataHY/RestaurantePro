using RestaurantePro.Mobile.Core.Models.DTOs;

namespace RestaurantePro.Mobile.Core.Services.Dashboard;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene las ventas del día actual
    /// </summary>
    Task<decimal> GetTodaySalesAsync();
    
    /// <summary>
    /// Obtiene el porcentaje de cambio de ventas vs ayer
    /// </summary>
    Task<decimal> GetSalesChangePercentageAsync();
    
    /// <summary>
    /// Obtiene el número de comandas activas
    /// </summary>
    Task<int> GetActiveOrdersCountAsync();
    
    /// <summary>
    /// Obtiene el número de comandas pendientes
    /// </summary>
    Task<int> GetPendingOrdersCountAsync();
    
    /// <summary>
    /// Obtiene las comandas recientes
    /// </summary>
    Task<List<OrderItem>> GetRecentOrdersAsync();
    
    /// <summary>
    /// Obtiene las comandas por estado
    /// </summary>
    Task<List<OrderItem>> GetOrdersByStatusAsync(string status);

    /// <summary>
    /// Obtiene el estado actual de todas las mesas
    /// </summary>
    /// <returns>Estado completo de las mesas</returns>
    Task<EstadoMesasDto> GetTableStatusAsync();
}
