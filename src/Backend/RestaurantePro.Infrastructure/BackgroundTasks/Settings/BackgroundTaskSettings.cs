namespace RestaurantePro.Infrastructure.BackgroundTasks.Settings;

/// <summary>
/// Clase principal para la configuración de todas las tareas en segundo plano.
/// </summary>
public class BackgroundTaskSettings
{
    public OperacionesSettings Operaciones { get; set; } = new();
} 