namespace RestaurantePro.Infrastructure.BackgroundTasks.Settings;

/// <summary>
/// Configuración para el trabajo de limpieza de mesas.
/// </summary>
public class TableCleanupJobSettings
{
    /// <summary>
    /// El número de minutos que una mesa debe estar en estado 'PendienteLimpieza' para ser considerada 'sucia'.
    /// </summary>
    public int StaleTimeMinutes { get; set; } = 30;
} 