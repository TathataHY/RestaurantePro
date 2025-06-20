namespace RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

/// <summary>
/// Interface base para todos los seeders de datos
/// </summary>
public interface ISeedData
{
    /// <summary>
    /// Nombre del seeder para logging
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Orden de ejecución (menor valor = mayor prioridad)
    /// </summary>
    int Order { get; }
    
    /// <summary>
    /// Indica si este seeder debe ejecutarse solo en desarrollo
    /// </summary>
    bool IsDevOnly { get; }
    
    /// <summary>
    /// Indica si este seeder es crítico para el funcionamiento del sistema
    /// </summary>
    bool IsCritical { get; }
    
    /// <summary>
    /// Siembra los datos en la base de datos
    /// </summary>
    /// <param name="context">Contexto de la base de datos</param>
    /// <param name="logger">Logger para registro de operaciones</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Tarea asíncrona</returns>
    Task SeedAsync(RestauranteProDbContext context, ILogger logger, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si los datos ya existen en la base de datos
    /// </summary>
    /// <param name="context">Contexto de la base de datos</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si los datos ya existen, false en caso contrario</returns>
    Task<bool> ExistsAsync(RestauranteProDbContext context, CancellationToken cancellationToken = default);
} 