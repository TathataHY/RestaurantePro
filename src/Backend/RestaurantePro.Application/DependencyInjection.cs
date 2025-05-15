using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Features.Inventario.Services;

namespace RestaurantePro.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            services.AddAutoMapper(assembly);
            services.AddValidatorsFromAssembly(assembly);
            services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

            // Registrar servicios de aplicación
            services.AddTransient<IInventarioComandaService, InventarioComandaService>();

            return services;
        }
    }
} 