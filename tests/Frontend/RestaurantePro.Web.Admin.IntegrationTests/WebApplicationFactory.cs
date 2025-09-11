using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Data.Common;

namespace RestaurantePro.Web.Admin.IntegrationTests;

/// <summary>
/// Factory para crear instancias de la aplicación web administrativa para pruebas de integración
/// </summary>
public class WebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover la base de datos real
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Agregar base de datos en memoria para pruebas
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Configurar logging para pruebas
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // Seed datos de prueba
        await SeedTestDataAsync(context);
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        await context.Database.EnsureDeletedAsync();
        
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        
        await base.DisposeAsync();
    }

    private async Task SeedTestDataAsync(RestauranteProDbContext context)
    {
        // Aquí puedes agregar datos de prueba específicos para las pruebas de integración
        // Por ejemplo, usuarios de prueba, productos, etc.
        
        await context.SaveChangesAsync();
    }
}
