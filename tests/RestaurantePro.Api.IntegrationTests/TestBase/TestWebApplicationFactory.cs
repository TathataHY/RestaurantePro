using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Core.Productos.Builders;

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
            
            // Agregar configuración específica para tests que FUERZA InMemory
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemoryDatabase",
                ["UseInMemoryDatabase"] = "true",  // Flag para Infrastructure
                ["Logging:LogLevel:Default"] = "Error",      // Solo errores en tests
                ["Logging:LogLevel:Microsoft"] = "Error",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Error"
            });
            
            // Configurar variable para modo testing
            Environment.SetEnvironmentVariable("TESTING_MODE", "true");
            
            // Agregar variables de entorno para tests
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // 🚀 CONFIGURACIÓN PARA TESTS: Infrastructure está desactivada por Program.cs
            // Necesitamos registrar servicios mínimos necesarios
            
            // Configurar ÚNICAMENTE InMemory para tests
            var dbName = "TestDatabase_" + Guid.NewGuid().ToString();
            
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Registrar IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // 🔧 REGISTRAR SERVICIOS BÁSICOS QUE APPLICATION NECESITA
            // ITimeProvider - necesario para PerformanceBehavior
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            
            // IDelayProvider - necesario para RetryBehavior
            services.AddSingleton<IDelayProvider, DelayProvider>();
            
            // 📦 REGISTRAR REPOSITORIOS NECESARIOS PARA LOS TESTS
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            // 🏗️ REGISTRAR BUILDERS DE DOMAIN
            services.AddScoped<ProductoBuilder>();
            
            // Otros servicios básicos que podrían ser necesarios
            // services.AddSingleton<ICacheService, InMemoryCacheService>();
            // services.AddScoped<ICurrentUserService, TestCurrentUserService>();

            // Configurar logging mínimo para tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Error);
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Para InMemory database no necesitamos limpiar manualmente
            // ya que se desecha automáticamente al finalizar el test
        }
        
        base.Dispose(disposing);
    }
} 