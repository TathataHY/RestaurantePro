using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RestaurantePro.Infrastructure.Persistence.Interceptors;

namespace RestaurantePro.Infrastructure.Persistence.Contexts;

public class ProveedoresDbContextFactory : IDesignTimeDbContextFactory<ProveedoresDbContext>
{
    public ProveedoresDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProveedoresDbContext>();
        
        // Configurar connection string
        var connectionString = "Server=TATHATA\\SQLEXPRESS;Database=RestauranteProProveedoresDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        
        optionsBuilder.UseSqlServer(connectionString, 
            b => b.MigrationsAssembly(typeof(ProveedoresDbContext).Assembly.FullName)
                  .MigrationsHistoryTable("__EFMigrationsHistory", "Proveedores"));
        
        // Crear servicios mock para el contexto
        var logger = new MockLogger<ProveedoresDbContext>();
        var currentUserService = new MockCurrentUserService();
        var dateTimeService = new MockDateTimeService();
        var domainEventDispatcher = new MockDomainEventDispatcher();
        var auditableEntityInterceptor = new AuditableEntityInterceptor(currentUserService, dateTimeService, new MockLogger<AuditableEntityInterceptor>());
        var domainEventInterceptor = new DomainEventInterceptor(domainEventDispatcher, new MockLogger<DomainEventInterceptor>());
        var softDeleteInterceptor = new SoftDeleteInterceptor(dateTimeService, currentUserService, new MockLogger<SoftDeleteInterceptor>());
        
        return new ProveedoresDbContext(
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