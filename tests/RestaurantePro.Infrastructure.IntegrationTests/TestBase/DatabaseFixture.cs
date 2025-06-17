using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class DatabaseFixture : IDisposable
    {
        private readonly DbConnection _connection;
        public DbConnection Connection => _connection;

        public DatabaseFixture()
        {
            // Using a shared in-memory database ensures the database persists as long as one connection is open.
            _connection = new SqliteConnection("DataSource=TestDatabase;Mode=Memory;Cache=Shared");
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }

    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
} 