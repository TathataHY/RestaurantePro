using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Interfaces.Repositories;
using RestaurantePro.Domain.Interfaces.Services;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Repositories;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Infrastructure.Services.BackgroundServices;

namespace RestaurantePro.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHashService, PasswordHashService>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUsuarioActualService, UsuarioActualService>();
            
            // Repositorios
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            
            // Servicios en segundo plano
            services.AddHostedService<ProcesadorInventarioBackgroundService>();
            
            return services;
        }
    }
} 