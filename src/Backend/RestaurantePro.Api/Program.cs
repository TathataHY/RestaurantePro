using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantePro.Api.Extensions;
using RestaurantePro.Api.Middlewares;
using RestaurantePro.Application;
using RestaurantePro.Domain.Entities;
using RestaurantePro.Infrastructure;
using RestaurantePro.Infrastructure.Persistence;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantePro.Api.Middleware;
using RestaurantePro.Application.Config.DependencyInjection;
using RestaurantePro.Infrastructure.DependencyInjection;
using Hangfire.Dashboard;

namespace RestaurantePro.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Agregar servicios al contenedor
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            
            // Configuración específica para la API
            builder.Services.AddApiServices();
            
            // Agregar capas inferiores
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            
            var app = builder.Build();
            
            // Configurar el pipeline de solicitudes HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            // Middleware global para manejo de excepciones
            app.UseMiddleware<ExceptionMiddleware>();
            
            app.UseHttpsRedirection();
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Habilitar dashboard de Hangfire (opcional)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            
            // Programar los trabajos recurrentes
            app.UseHangfireJobs();
            
            app.MapControllers();
            
            app.Run();
        }
    }
}
