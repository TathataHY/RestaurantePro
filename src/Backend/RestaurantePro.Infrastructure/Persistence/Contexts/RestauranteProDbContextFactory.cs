using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

public class RestauranteProDbContextFactory : IDesignTimeDbContextFactory<RestauranteProDbContext>
{
    public RestauranteProDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RestauranteProDbContext>();
        
        // Leer configuración desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString, 
            b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)
                  .MigrationsHistoryTable("__EFMigrationsHistory", "Core"));
        
        // Crear servicios mock para el contexto
        var logger = new MockLogger<RestauranteProDbContext>();
        var domainEventDispatcher = new MockDomainEventDispatcher();
        
        return new RestauranteProDbContext(
            optionsBuilder.Options,
            logger,
            domainEventDispatcher);
    }
} 