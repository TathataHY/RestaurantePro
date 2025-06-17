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

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public class DatabaseFixture : IDisposable
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly RestauranteProDbContext DbContext;
        private readonly string _databaseName;

        public DatabaseFixture()
        {
            _databaseName = $"RestauranteProTest_{Guid.NewGuid()}";
            var services = new ServiceCollection();
            
            var mockCurrentUserService = new Mock<ICurrentUserService>();
            var mockDateTimeService = new Mock<IDateTimeService>();
            var mockDomainEventDispatcher = new Mock<IDomainEventDispatcher>();

            services.AddSingleton(mockCurrentUserService.Object);
            services.AddSingleton(mockDateTimeService.Object);
            services.AddSingleton(mockDomainEventDispatcher.Object);

            // Registrar Interceptor y Logger
            services.AddSingleton<AuditableEntityInterceptor>();
            services.AddSingleton(Mock.Of<ILogger<RestauranteProDbContext>>());

            // Configuración de la base de datos en memoria para pruebas
            services.AddDbContext<RestauranteProDbContext>((sp, options) =>
                options.UseInMemoryDatabase(_databaseName)
                       .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

            // Registrar servicios adicionales necesarios
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<RestauranteProDbContext>();

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