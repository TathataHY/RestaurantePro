using Microsoft.Data.Sqlite;
using System;
using System.Data.Common;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class DatabaseFixture : IDisposable
    {
        private const string ConnectionString = "DataSource=:memory:;Cache=Shared";
        private IServiceScope _serviceScope;
        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public SqliteConnection Connection { get; }

        public DatabaseFixture()
        {
            // Using a shared in-memory database ensures the database persists as long as one connection is open.
            Connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
            Connection.Open();

            var services = new ServiceCollection();

            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlite(ConnectionString));
            
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<RestauranteProDbContext>());

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            _serviceScope = services.BuildServiceProvider().CreateScope();
        }

        public void Dispose()
        {
            _serviceScope?.Dispose();
            Connection.Dispose();
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