using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantePro.Api.Extensions;
using RestaurantePro.Api.Configuration;
using RestaurantePro.Api.Middleware;
using RestaurantePro.Application;
using RestaurantePro.Infrastructure;
using RestaurantePro.Infrastructure.Persistence;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantePro.Application.Config.DependencyInjection;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Domain.Core;
using Hangfire.Dashboard;

namespace RestaurantePro.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Agregar servicios al contenedor
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            
            // Configurar Swagger usando la clase de configuración
            builder.Services.ConfigureSwagger();
            
            // Configuración específica para la API
            builder.Services.AddApiServices();
            
            // Configurar SeedData desde appsettings
            builder.Services.Configure<RestaurantePro.Infrastructure.Persistence.SeedData.Extensions.SeedDataConfiguration>(
                builder.Configuration.GetSection("SeedData"));
            
            // Agregar capas inferiores
            builder.Services.AddDomainServices();
            
            // ⚠️ DETECTAR MODO TESTING para evitar conflictos de DbContext
            var isTestingMode = Environment.GetEnvironmentVariable("TESTING_MODE") == "true" ||
                               builder.Environment.EnvironmentName == "Testing";
            
            if (!isTestingMode)
            {
                // En producción/desarrollo: usar Infrastructure completa
                builder.Services.AddInfrastructureServices(builder.Configuration);
            }
            // En testing: Infrastructure será configurada por TestWebApplicationFactory
            
            builder.Services.AddApplicationServices(builder.Configuration);
            
            var app = builder.Build();
            
            // 🔧 CONFIGURAR BASE DE DATOS Y SEED DATA
            // Ejecutar solo seed data SIN migraciones (las tablas ya existen)
            if ((app.Environment.IsDevelopment() || app.Environment.IsStaging()) && !isTestingMode)
            {
                await app.UseSeedDataAsync(shouldMigrate: false); // Solo datos, NO migraciones
            }
            
            // Configurar el pipeline de solicitudes HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.ConfigureSwaggerUI();
            }
            
            // Middleware global para manejo de excepciones
            app.UseMiddleware<ExceptionMiddleware>();
            
            app.UseHttpsRedirection();
            
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            // TODO: Agregar paquetes de Hangfire  
            // Habilitar dashboard de Hangfire (opcional)
            // app.UseHangfireDashboard("/hangfire", new DashboardOptions
            // {
            //     Authorization = new[] { new HangfireAuthorizationFilter() }
            // });
            
            // Programar los trabajos recurrentes
            // app.UseHangfireJobs();
            
            app.MapControllers();
            
            await app.RunAsync();
        }
    }
}
