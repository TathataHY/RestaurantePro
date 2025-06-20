using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Interfaces;
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
        var logger = new MockLogger();
        var domainEventDispatcher = new MockDomainEventDispatcher();
        
        return new RestauranteProDbContext(
            optionsBuilder.Options,
            logger,
            domainEventDispatcher);
    }
}

// Servicios mock para design time
public class MockLogger : ILogger<RestauranteProDbContext>
{
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

public class MockDomainEventDispatcher : IDomainEventDispatcher
{
    public Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
} 