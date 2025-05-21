using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extensiones para registrar servicios de infraestructura en la inyección de dependencias
    /// </summary>
    public static class InfrastructureServiceCollectionExtensions
    {
        /// <summary>
        /// Agrega los servicios de infraestructura a la colección de servicios
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>La colección de servicios con los servicios de infraestructura agregados</returns>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar contexto de base de datos
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));

            // Registrar Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Registrar repositorios
            services.AddRepositories();

            return services;
        }

        /// <summary>
        /// Registra todos los repositorios en la colección de servicios
        /// </summary>
        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // Repositorio genérico
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Repositorios específicos de contexto
            
            // Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
            
            // Inventario
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            
            // Aquí se agregarán los demás repositorios específicos a medida que sean implementados
            
            return services;
        }
    }

    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
} 