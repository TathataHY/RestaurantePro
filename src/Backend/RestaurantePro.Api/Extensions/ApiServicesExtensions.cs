using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Filters;
using RestaurantePro.Api.Middleware;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Api.Services;

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
                // Convertir enums a strings
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // Configurar reglas de validación de modelo
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(e => e.Value.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    var response = ApiResponse<object>.ErrorResponse(errors, "Error de binding o validación", 400);
                    return new BadRequestObjectResult(response);
                };
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
                
                // Política más segura para producción
                options.AddPolicy("Production", builder =>
                {
                    builder.WithOrigins(
                            "https://limoncitoydedos-001-site1.site4now.net",
                            "https://www.limoncitoydedos-001-site1.site4now.net",
                            "http://localhost:3000",
                            "http://localhost:8080"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // 🚀 Registrar servicios de SignalR
            services.AddScoped<ISignalRHub, SignalRHubService>();

            return services;
        }
    }
} 