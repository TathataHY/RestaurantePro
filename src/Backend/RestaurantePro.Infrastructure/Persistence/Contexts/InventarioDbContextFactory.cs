using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

public class InventarioDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventarioDbContext>();
        
        // Leer configuración desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString, 
            b => b.MigrationsAssembly(typeof(InventarioDbContext).Assembly.FullName)
                  .MigrationsHistoryTable("__EFMigrationsHistory", "Inventario"));
        
        // Crear servicios mock para el contexto
        var logger = new MockLogger<InventarioDbContext>();
        var currentUserService = new MockCurrentUserService();
        var dateTimeService = new MockDateTimeService();
        var domainEventDispatcher = new MockDomainEventDispatcher();
        var auditableEntityInterceptor = new AuditableEntityInterceptor(currentUserService, dateTimeService, new MockLogger<AuditableEntityInterceptor>());
        var domainEventInterceptor = new DomainEventInterceptor(domainEventDispatcher, new MockLogger<DomainEventInterceptor>());
        var softDeleteInterceptor = new SoftDeleteInterceptor(dateTimeService, currentUserService, new MockLogger<SoftDeleteInterceptor>());
        
        return new InventarioDbContext(
            optionsBuilder.Options,
            currentUserService,
            dateTimeService,
            domainEventDispatcher,
            logger,
            auditableEntityInterceptor,
            domainEventInterceptor,
            softDeleteInterceptor);
    }
} 