using Microsoft.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Api.Extensions;

/// <summary>
/// Extensiones para configurar seed data en el startup de la aplicación
/// </summary>
public static class SeedDataExtensions
{
    /// <summary>
    /// Configura y ejecuta todos los seeders al iniciar la aplicación
    /// </summary>
    /// <param name="app">La aplicación web</param>
    /// <param name="shouldMigrate">Si debe ejecutar migraciones automáticamente</param>
    /// <returns>La aplicación web</returns>
    public static async Task<WebApplication> UseSeedDataAsync(this WebApplication app, bool shouldMigrate = true)
    {
        var logger = app.Services.GetRequiredService<ILogger<SeedDataRunner>>();
        
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        try
        {
            logger.LogInformation("🚀 Iniciando configuración de base de datos y seed data...");
            
            // 1. MIGRAR BASE DE DATOS (si es necesario)
            if (shouldMigrate)
            {
                logger.LogInformation("📦 Aplicando migraciones de base de datos...");
                await context.Database.MigrateAsync();
                logger.LogInformation("✅ Migraciones aplicadas correctamente");
            }
            
            // 2. EJECUTAR SEED DATA
            var seedRunner = scope.ServiceProvider.GetRequiredService<SeedDataRunner>();
            await seedRunner.RunSeedersAsync();
            
            logger.LogInformation("🎉 Base de datos y seed data configurados exitosamente");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error al configurar base de datos y seed data");
            
            // En desarrollo, podemos continuar, en producción podrías querer fallar
            if (app.Environment.IsProduction())
            {
                throw;
            }
        }
        
        return app;
    }
    
    /// <summary>
    /// Versión síncrona para casos especiales
    /// </summary>
    /// <param name="app">La aplicación web</param>
    /// <param name="shouldMigrate">Si debe ejecutar migraciones automáticamente</param>
    /// <returns>La aplicación web</returns>
    public static WebApplication UseSeedData(this WebApplication app, bool shouldMigrate = true)
    {
        return app.UseSeedDataAsync(shouldMigrate).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Configura seed data solo para entornos específicos
    /// </summary>
    /// <param name="app">La aplicación web</param>
    /// <param name="environments">Entornos donde debe ejecutarse</param>
    /// <returns>La aplicación web</returns>
    public static async Task<WebApplication> UseSeedDataForEnvironmentsAsync(
        this WebApplication app, 
        params string[] environments)
    {
        if (environments.Contains(app.Environment.EnvironmentName, StringComparer.OrdinalIgnoreCase))
        {
            await app.UseSeedDataAsync();
        }
        
        return app;
    }
} 