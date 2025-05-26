namespace RestaurantePro.Domain.Core.SharedKernel.Validation;

/// <summary>
/// Extensiones para registrar los servicios de validación en el contenedor de dependencias
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de validación en el contenedor de dependencias
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>La colección de servicios con los servicios de validación registrados</returns>
    public static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        // Registrar como Scoped para que cada petición tenga su propio NotificationManager
        services.AddScoped<INotificationManager, NotificationManager>();
        
        return services;
    }
} 