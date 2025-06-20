using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Factory personalizada para configurar la aplicación web en los tests de integración.
/// Sobrescribe la configuración de producción para usar base de datos en memoria.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuraciones existentes
            config.Sources.Clear();
            
            // Agregar configuración específica para tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["Logging:LogLevel:Default"] = "Information",
                ["Logging:LogLevel:Microsoft"] = "Warning",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information"
            });
            
            // Agregar variables de entorno para tests
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // Remover la configuración de base de datos existente
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Agregar base de datos en memoria para tests
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Configurar logging para tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Asegurar que la base de datos se crea
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Limpiar recursos si es necesario
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Database.EnsureDeleted();
        }
        
        base.Dispose(disposing);
    }
} 