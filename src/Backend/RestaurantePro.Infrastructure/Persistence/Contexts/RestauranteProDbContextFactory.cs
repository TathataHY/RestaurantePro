using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

public class RestauranteProDbContextFactory : IDesignTimeDbContextFactory<RestauranteProDbContext>
{
    public RestauranteProDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RestauranteProDbContext>();
        
        // Configurar connection string
        var connectionString = "Server=TATHATA\\SQLEXPRESS;Database=RestauranteProDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        
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