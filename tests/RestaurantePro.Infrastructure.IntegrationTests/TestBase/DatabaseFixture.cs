using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using System;
using Xunit;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Infrastructure.DependencyInjection;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class DatabaseFixture : IDisposable
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IServiceScopeFactory ScopeFactory;
        public readonly TestDbContext DbContext;
        private readonly string _databaseName;
        private readonly IConfiguration _configuration;

        public DatabaseFixture()
        {
            _databaseName = $"TestDb_{Guid.NewGuid()}";

            var services = new ServiceCollection();
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "JwtSettings:Secret", "super-secret-key-for-jwt-that-is-long" },
                    { "JwtSettings:Issuer", "RestaurantePro.Test" },
                    { "JwtSettings:Audience", "RestaurantePro.Test" }
                }!).Build();

            var mockCurrentUserService = new Mock<ICurrentUserService>();
            var mockDateTimeService = new Mock<IDateTimeService>();
            var mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            services.AddSingleton(mockCurrentUserService.Object);
            services.AddSingleton(mockDateTimeService.Object);
            services.AddSingleton(mockDomainEventDispatcher.Object);

            // Registrar Interceptor y Logger
            services.AddSingleton<AuditableEntityInterceptor>();
            services.AddSingleton(Mock.Of<ILogger<AuditableEntityInterceptor>>());
            services.AddSingleton(Mock.Of<ILogger<RestauranteProDbContext>>());

            // Configuración de la base de datos en memoria para pruebas
            services.AddDbContext<TestDbContext>((sp, options) =>
                options.UseInMemoryDatabase(_databaseName)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

            // Registrar DbContext base para que UnitOfWork pueda resolverlo
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
            services.AddScoped<RestauranteProDbContext>(sp => sp.GetRequiredService<TestDbContext>());

            // Registrar servicios de infraestructura
            services.AddInfrastructureServices(_configuration, isTestEnvironment: true);

            // Registrar servicios adicionales necesarios
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            ScopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            DbContext = ServiceProvider.GetRequiredService<TestDbContext>();

            // Inicializar con datos de prueba
            SeedDatabase();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Los servicios adicionales se configurarán en las clases derivadas
        }

        protected virtual void SeedDatabase()
        {
            // Datos de prueba básicos
            // Ejemplo: DbContext.Productos.Add(new Producto { ... });
            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }

    // Interfaz para colecciones de pruebas que necesiten compartir la misma base de datos
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // Esta clase no tiene código, solo se usa para definir la colección
    }
} 