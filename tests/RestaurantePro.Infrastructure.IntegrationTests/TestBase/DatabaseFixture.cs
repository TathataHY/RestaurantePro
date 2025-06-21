using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using NSubstitute;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

/// <summary>
/// Fixture para proporcionar una base de datos de prueba compartida
/// </summary>
public class DatabaseFixture : IDisposable
{
    public RestauranteProDbContext DbContext { get; }

    public DatabaseFixture()
    {
        // Configurar SQLite in-memory para soporte de transacciones reales
        var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        // Crear mocks para los servicios requeridos
        var logger = Substitute.For<ILogger<RestauranteProDbContext>>();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        DbContext = new RestauranteProDbContext(options, logger, dispatcher);
        DbContext.Database.OpenConnection();
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
    }
}

[CollectionDefinition("DatabaseCollection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
} 