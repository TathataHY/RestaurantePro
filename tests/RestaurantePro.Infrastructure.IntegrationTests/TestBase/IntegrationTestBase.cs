using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly RestauranteProDbContext DbContext;

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();

            // Registrar servicios para pruebas
            services.AddLogging(builder => builder.AddConsole());
            
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            // Registrar otros servicios necesarios para las pruebas
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<RestauranteProDbContext>();

            // Inicializar la base de datos con datos de prueba
            SeedDatabase();
        }

        // Método para registrar servicios adicionales (a ser implementado por clases derivadas)
        protected virtual void RegisterServices(IServiceCollection services)
        {
        }

        // Método para sembrar datos de prueba (a ser implementado por clases derivadas)
        protected virtual void SeedDatabase()
        {
        }

        // Método para que las clases derivadas configuren servicios adicionales
        protected virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
            (ServiceProvider as IDisposable)?.Dispose();
        }
    }
} 