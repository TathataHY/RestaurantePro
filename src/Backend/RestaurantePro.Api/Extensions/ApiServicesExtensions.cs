using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Filters;
using RestaurantePro.Api.Middleware;

namespace RestaurantePro.Api.Extensions
{
    /// <summary>
    /// Extensiones para configurar servicios específicos de la API
    /// </summary>
    public static class ApiServicesExtensions
    {
        /// <summary>
        /// Agrega los servicios específicos de la API
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Configurar controladores con opciones específicas
            services.AddControllers(options =>
            {
                options.Filters.Add<ApiExceptionFilterAttribute>();
            })
            .AddJsonOptions(options =>
            {
                // Ignorar referencias circulares en JSON
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                // Usar nombres de propiedades en camelCase
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                // Ignorar valores nulos
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Configurar reglas de validación de modelo
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Configurar CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // TODO: Agregar paquetes de Swagger - Comentado temporalmente
            // Agregar Swagger
            // services.AddEndpointsApiExplorer();
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            //     {
            //         Title = "RestaurantePro API",
            //         Version = "v1",
            //         Description = "API para la gestión de restaurante"
            //     });
            // });

            return services;
        }
    }
} 