using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Interfaces.Repositories;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using System;
using RestaurantePro.Infrastructure.Persistence.Base;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Configuración de los servicios de persistencia
    /// </summary>
    public static class PersistenceSetup
    {
        /// <summary>
        /// Registra los servicios de persistencia en el contenedor de inyección de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Colección de servicios con los servicios de persistencia registrados</returns>
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar interceptores
            services.AddScoped<AuditableEntityInterceptor>();
            services.AddScoped<DomainEventInterceptor>();
            services.AddScoped<SoftDeleteInterceptor>();

            // Configuración de la base de datos
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));

            // Registrar interfaces de aplicación
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // Registrar Unit of Work
            services.AddScoped<UnitOfWork>();

            // Registrar repositorios
            RegisterRepositories(services);

            return services;
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            // Repositorio base genérico
            services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

            // Repositorios específicos
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            // Añadir más repositorios aquí a medida que se implementen...
        }
    }
} 