using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using NSubstitute;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

/// <summary>
/// Fixture para proporcionar una base de datos de prueba única por test
/// </summary>
public class DatabaseFixture : IDisposable
{
    private SqliteConnection? _connection;
    private TestRestauranteProDbContext? _dbContext;

    public TestRestauranteProDbContext CreateDbContext()
    {
        // Crear una conexión SQLite in-memory única para cada test
        _connection = new SqliteConnection($"DataSource=test_{Guid.NewGuid()}.db;Mode=Memory;Cache=Shared");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RestauranteProDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Crear mocks para los servicios requeridos
        var logger = Substitute.For<ILogger<RestauranteProDbContext>>();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        _dbContext = new TestRestauranteProDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        return _dbContext;
    }

    public SqliteConnection CreateConnection()
    {
        // Crear una conexión SQLite in-memory única para cada test
        _connection = new SqliteConnection($"DataSource=test_{Guid.NewGuid()}.db;Mode=Memory;Cache=Shared");
        _connection.Open();
        return _connection;
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _connection?.Dispose();
    }
}

[CollectionDefinition("DatabaseCollection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
} 