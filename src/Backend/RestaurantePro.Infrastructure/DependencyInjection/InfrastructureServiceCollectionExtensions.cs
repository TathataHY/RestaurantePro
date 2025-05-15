using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Base;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;

namespace RestaurantePro.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar base de datos
            services.AddDbContext<RestauranteProDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(RestauranteProDbContext).Assembly.FullName)));

            // Interfaces comunes de infraestructura
            services.AddScoped<IDateTime, DateTimeService>();
            
            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositorios - Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
            
            // Repositorios - Operaciones
            // services.AddScoped<IComandaRepository, ComandaRepository>();
            // services.AddScoped<IMesaRepository, MesaRepository>();
            
            // Servicios externos
            // services.AddTransient<IEmailService, EmailService>();
            // services.AddTransient<ISmsService, SmsService>();

            return services;
        }
    }

    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
    }
} 