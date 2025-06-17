using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using System;
using Scrutor;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Respawn;
using System.Linq;
using System.Collections.Generic;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public abstract class IntegrationTestBase : IClassFixture<DatabaseFixture>, IDisposable
    {
        private readonly DatabaseFixture _fixture;
        protected readonly IServiceScope ServiceScope;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly TestDbContext DbContext;
        // private readonly Respawn.Checkpoint _checkpoint;

        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
            ServiceScope = _fixture.ScopeFactory.CreateScope();
            ServiceProvider = ServiceScope.ServiceProvider;
            DbContext = ServiceProvider.GetRequiredService<TestDbContext>();

            /* _checkpoint = new Respawn.Checkpoint
            {
                TablesToIgnore = new[] { "__EFMigrationsHistory" },
                SchemasToInclude = new[] { "Core", "Comercial", "Inventario", "Operaciones", "Proveedores" },
                DbAdapter = DbAdapter.SqlServer(DatabaseFixture.ConnectionString)
            }; */
        }

        protected async Task ResetDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        protected virtual Task SeedDataForTestsAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            ResetDatabaseAsync().GetAwaiter().GetResult();
            ServiceScope.Dispose();
        }
    }
} 