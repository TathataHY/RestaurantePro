using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

public class OperacionesDbContextFactory : IDesignTimeDbContextFactory<OperacionesDbContext>
{
    public OperacionesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OperacionesDbContext>();
        
        // Leer configuración desde appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString, 
            b => b.MigrationsAssembly(typeof(OperacionesDbContext).Assembly.FullName)
                  .MigrationsHistoryTable("__EFMigrationsHistory", "Operaciones"));
        
        // Crear servicios mock para el contexto
        var logger = new MockLogger<OperacionesDbContext>();
        var currentUserService = new MockCurrentUserService();
        var dateTimeService = new MockDateTimeService();
        var domainEventDispatcher = new MockDomainEventDispatcher();
        var auditableEntityInterceptor = new AuditableEntityInterceptor(currentUserService, dateTimeService, new MockLogger<AuditableEntityInterceptor>());
        var domainEventInterceptor = new DomainEventInterceptor(domainEventDispatcher, new MockLogger<DomainEventInterceptor>());
        var softDeleteInterceptor = new SoftDeleteInterceptor(dateTimeService, currentUserService, new MockLogger<SoftDeleteInterceptor>());
        
        return new OperacionesDbContext(
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