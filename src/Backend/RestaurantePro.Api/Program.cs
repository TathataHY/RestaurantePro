using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RestaurantePro.Api.Extensions;
using RestaurantePro.Api.Configuration;
using RestaurantePro.Api.Middleware;
using RestaurantePro.Api.Hubs;
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
using Microsoft.AspNetCore.SignalR;

namespace RestaurantePro.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 🌍 CONFIGURAR ZONA HORARIA DE CHILE GLOBALMENTE
            ConfigurarZonaHorariaChile();
            
            var builder = WebApplication.CreateBuilder(args);

            // Agregar servicios al contenedor
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            
            // Configurar Swagger usando la clase de configuración
            builder.Services.ConfigureSwagger();
            
            // 🚀 CONFIGURAR SIGNALR PRIMERO (antes de otros servicios)
            builder.Services.AddSignalR(options =>
            {
                // Configuración para desarrollo
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableDetailedErrors = true;
                }
                
                // Configuración de keep-alive
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                
                // Configuración de límites
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
                options.MaximumParallelInvocationsPerClient = 1;
            });
            
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
                // Log para confirmar que se está ejecutando
                
                // En producción/desarrollo: usar Infrastructure completa
                builder.Services.AddInfrastructureServices(builder.Configuration);
                
                
                // En producción/desarrollo: usar Application Services completa
                builder.Services.AddApplicationServices(builder.Configuration);
            }
            // En testing: Infrastructure y Application Services serán configurados por TestWebApplicationFactory
            
            var app = builder.Build();
            
            // 🔧 CONFIGURAR BASE DE DATOS Y SEED DATA
            // Ejecutar solo seed data SIN migraciones (las tablas ya existen)
            // 🚀 FORZAR ejecución de seed data en TODOS los entornos excepto testing
            if (!isTestingMode)
            {
                if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
                {
                    await app.UseSeedDataAsync(shouldMigrate: false); // Solo datos, NO migraciones
                }
                else
                {
                    // En Beta/Production: Solo seed data (las tablas ya existen)
                    await app.UseSeedDataAsync(shouldMigrate: false); // Solo datos, NO migraciones
                }
            }
            
            // Configurar el pipeline de solicitudes HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.ConfigureSwaggerUI();
            }
            
            // Middleware global para manejo de excepciones
            app.UseMiddleware<ExceptionMiddleware>();
            
            // app.UseHttpsRedirection(); // Comentado para permitir HTTP en desarrollo
            
            app.UseRouting();
            
            // 📁 CONFIGURAR ARCHIVOS ESTÁTICOS PARA IMÁGENES
            app.UseStaticFiles();
            
            // 🔧 CONFIGURAR CORS - PERMITIR ACCESO DESDE EMULADORES ANDROID
            app.UseCors("AllowAll");
            
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
            
            // 🚀 MAPEAR SIGNALR HUBS
            app.MapHub<ComandaHub>("/hubs/comandas");
            app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<InventarioHub>("/hubs/inventario");
            
            await app.RunAsync();
        }

        /// <summary>
        /// Configura la zona horaria de Chile para toda la aplicación
        /// </summary>
        private static void ConfigurarZonaHorariaChile()
        {
            try
            {
                TimeZoneInfo chileTimeZone;
                try
                {
                    // Intentar obtener la zona horaria de Chile
                    chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
                }
                catch
                {
                    // En Windows, el ID puede ser diferente
                    try
                    {
                        chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
                    }
                    catch
                    {
                        // Fallback: crear manualmente la zona horaria de Chile
                        chileTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                            "Chile Standard Time", 
                            TimeSpan.FromHours(-3), 
                            "Chile Standard Time", 
                            "Chile Standard Time");
                    }
                }
                
                // Configurar variables de entorno para zona horaria
                Environment.SetEnvironmentVariable("TZ", "America/Santiago");
                
                // Log de configuración
                var utcNow = DateTime.UtcNow;
                var chileNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, chileTimeZone);
                
                Console.WriteLine($"🌍 Zona horaria configurada: {chileTimeZone.DisplayName}");
                Console.WriteLine($"🕐 Hora UTC: {utcNow:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"🕐 Hora Chile: {chileNow:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"🌍 Diferencia: {chileTimeZone.GetUtcOffset(utcNow)} horas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error configurando zona horaria de Chile: {ex.Message}");
                Console.WriteLine($"⚠️ Usando zona horaria del sistema por defecto");
            }
        }
    }
}
