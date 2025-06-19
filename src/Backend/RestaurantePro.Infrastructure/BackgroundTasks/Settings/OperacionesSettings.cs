namespace RestaurantePro.Infrastructure.BackgroundTasks.Settings;

/// <summary>
/// Contiene las configuraciones para los jobs del contexto de Operaciones.
/// </summary>
public class OperacionesSettings
{
    public TableCleanupJobSettings TableCleanupJob { get; set; } = new();
} 