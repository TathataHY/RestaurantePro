using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

/// <summary>
/// Configuración para el seed data
/// </summary>
public class SeedDataConfiguration
{
    public bool RunCriticalData { get; set; } = true;
    public bool RunDemoData { get; set; } = false;
    public bool RunTestingData { get; set; } = false;
    public bool CreateAdminUser { get; set; } = true;
    public string AdminEmail { get; set; } = "admin@restaurantepro.com";
    public string AdminPassword { get; set; } = "Admin@123";
    public bool ForceReseed { get; set; } = false;
}

/// <summary>
/// Runner principal para ejecutar todos los seeders de datos
/// </summary>
public class SeedDataRunner
{
    private readonly RestauranteProDbContext _context;
    private readonly ILogger<SeedDataRunner> _logger;
    private readonly SeedDataConfiguration _config;
    private readonly IServiceProvider _serviceProvider;

    public SeedDataRunner(
        RestauranteProDbContext context,
        ILogger<SeedDataRunner> logger,
        IOptions<SeedDataConfiguration> config,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _logger = logger;
        _config = config.Value;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Ejecuta todos los seeders configurados
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🌱 Iniciando proceso de seed data...");
        
        try
        {
            // Verificar conexión a la base de datos
            await VerificarConexionAsync(cancellationToken);
            
            // Obtener todos los seeders registrados
            var seeders = ObtenerSeeders();
            
            // Filtrar seeders según configuración
            var seedersAEjecutar = FiltrarSeeders(seeders);
            
            _logger.LogInformation("📊 Seeders a ejecutar: {Count}", seedersAEjecutar.Count);
            
            // Ejecutar seeders en orden
            await EjecutarSeedersAsync(seedersAEjecutar, cancellationToken);
            
            _logger.LogInformation("✅ Proceso de seed data completado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error durante el proceso de seed data");
            throw;
        }
    }

    /// <summary>
    /// Verifica la conexión a la base de datos
    /// </summary>
    private async Task VerificarConexionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔍 Verificando conexión a la base de datos...");
        
        try
        {
            await _context.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogInformation("✅ Conexión a la base de datos verificada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al conectar con la base de datos");
            throw;
        }
    }

    /// <summary>
    /// Obtiene todos los seeders registrados en el DI container
    /// </summary>
    private List<ISeedData> ObtenerSeeders()
    {
        var seeders = _serviceProvider.GetServices<ISeedData>().ToList();
        _logger.LogInformation("📋 Seeders encontrados: {Count}", seeders.Count);
        
        foreach (var seeder in seeders)
        {
            _logger.LogDebug("  - {Name} (Order: {Order}, Critical: {IsCritical}, DevOnly: {IsDevOnly})", 
                seeder.Name, seeder.Order, seeder.IsCritical, seeder.IsDevOnly);
        }
        
        return seeders;
    }

    /// <summary>
    /// Filtra los seeders según la configuración
    /// </summary>
    private List<ISeedData> FiltrarSeeders(List<ISeedData> seeders)
    {
        var filtrados = seeders.Where(s => DebeEjecutarse(s))
                              .OrderBy(s => s.Order)
                              .ThenBy(s => s.Name)
                              .ToList();
        
        _logger.LogInformation("🔍 Seeders filtrados: {Count} de {Total}", 
            filtrados.Count, seeders.Count);
            
        return filtrados;
    }

    /// <summary>
    /// Determina si un seeder debe ejecutarse según la configuración
    /// </summary>
    private bool DebeEjecutarse(ISeedData seeder)
    {
        // Datos críticos siempre se ejecutan si está habilitado
        if (seeder.IsCritical && _config.RunCriticalData)
            return true;
            
        // Datos de demo solo si está habilitado
        if (!seeder.IsCritical && !seeder.IsDevOnly && _config.RunDemoData)
            return true;
            
        // Datos de testing solo si está habilitado
        if (seeder.IsDevOnly && _config.RunTestingData)
            return true;
            
        return false;
    }

    /// <summary>
    /// Ejecuta los seeders en el orden especificado
    /// </summary>
    private async Task EjecutarSeedersAsync(List<ISeedData> seeders, CancellationToken cancellationToken)
    {
        var ejecutados = 0;
        var omitidos = 0;
        
        foreach (var seeder in seeders)
        {
            try
            {
                _logger.LogInformation("🌱 Ejecutando seeder: {Name}...", seeder.Name);
                
                // Verificar si ya existe (a menos que sea force reseed)
                if (!_config.ForceReseed && await seeder.ExistsAsync(_context, cancellationToken))
                {
                    _logger.LogInformation("⏭️  Datos ya existen para {Name}, omitiendo...", seeder.Name);
                    omitidos++;
                    continue;
                }
                
                // Usar transacción para cada seeder
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    await seeder.SeedAsync(_context, _logger, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    
                    ejecutados++;
                    _logger.LogInformation("✅ Seeder {Name} ejecutado exitosamente", seeder.Name);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "❌ Error en seeder {Name}", seeder.Name);
                    
                    if (seeder.IsCritical)
                    {
                        _logger.LogError("🚨 Seeder crítico falló, abortando proceso");
                        throw;
                    }
                    
                    _logger.LogWarning("⚠️  Seeder no crítico falló, continuando...");
                }
            }
            catch (Exception ex) when (!seeder.IsCritical)
            {
                _logger.LogWarning(ex, "⚠️  Error no crítico en seeder {Name}", seeder.Name);
            }
        }
        
        _logger.LogInformation("📊 Resumen: {Ejecutados} ejecutados, {Omitidos} omitidos", 
            ejecutados, omitidos);
    }

    /// <summary>
    /// Ejecuta solo los seeders críticos
    /// </summary>
    public async Task RunCriticalOnlyAsync(CancellationToken cancellationToken = default)
    {
        var originalConfig = _config.RunDemoData;
        var originalTesting = _config.RunTestingData;
        
        _config.RunDemoData = false;
        _config.RunTestingData = false;
        
        try
        {
            await RunAsync(cancellationToken);
        }
        finally
        {
            _config.RunDemoData = originalConfig;
            _config.RunTestingData = originalTesting;
        }
    }

    /// <summary>
    /// Limpia todos los datos y re-ejecuta los seeders
    /// </summary>
    public async Task CleanAndReseedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("🧹 Limpiando base de datos para re-seed...");
        
        _config.ForceReseed = true;
        
        try
        {
            await RunAsync(cancellationToken);
        }
        finally
        {
            _config.ForceReseed = false;
        }
    }
} 