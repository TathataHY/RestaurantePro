using Microsoft.OpenApi.Models;

namespace RestaurantePro.Api.Configuration;

/// <summary>
/// Configuración detallada de Swagger para la API de RestaurantePro
/// </summary>
public static class SwaggerConfig
{
    /// <summary>
    /// Configura Swagger y OpenAPI para la aplicación
    /// </summary>
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            // 📝 Información básica de la API
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RestaurantePro API",
                Version = "v1",
                Description = "API REST para gestión integral de restaurantes con arquitectura por contextos de dominio",
                Contact = new OpenApiContact
                {
                    Name = "RestaurantePro Development Team",
                    Email = "dev@restaurantepro.com",
                    Url = new Uri("https://github.com/restaurantepro/api")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // 🔐 Configuración de autenticación JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingresa 'Bearer' seguido de un espacio y el token JWT. Ejemplo: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'"
            });

            // 🔒 Requerimiento de seguridad global
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });

            // 📂 Organización por tags (contextos)
            c.TagActionsBy(api => new[] { GetControllerContext(api.ActionDescriptor.RouteValues["controller"] ?? "") });
            c.DocInclusionPredicate((name, api) => true);

            // 🏷️ Configuración de tags personalizados
            ConfigureCustomTags(c);

            // 📋 Configuraciones adicionales
            c.DescribeAllParametersInCamelCase();
            c.CustomSchemaIds(type => type.FullName);
        });
    }

    /// <summary>
    /// Configura la UI de Swagger con opciones personalizadas
    /// </summary>
    public static void ConfigureSwaggerUI(this WebApplication app)
    {
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestaurantePro API v1");
            c.RoutePrefix = "swagger"; // Accesible en /swagger
            
            // 🎨 Configuración de la interfaz
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DefaultModelsExpandDepth(-1); // No expandir modelos por defecto
            c.EnableDeepLinking();
            c.EnableFilter();
            c.ShowExtensions();
            c.EnableValidator();
            
            // 🏠 Configuración de la página principal
            c.DocumentTitle = "RestaurantePro API - Documentación";
            c.InjectStylesheet("/swagger-ui/custom.css");
            
            // 📱 Configuración responsive
            c.ConfigObject.AdditionalItems.Add("syntaxHighlight", new Dictionary<string, object>
            {
                ["activated"] = true,
                ["theme"] = "agate"
            });
        });
    }

    /// <summary>
    /// Obtiene el contexto del controlador para organización
    /// </summary>
    private static string GetControllerContext(string controllerName)
    {
        return controllerName switch
        {
            // 🏗️ Contexto Core
            "Productos" or "Usuarios" or "Notificaciones" or "Recetas" 
                => "🏗️ Core - Funcionalidades Centrales",
            
            // 🛒 Contexto Comercial  
            "Clientes" or "Facturas" or "TarjetasFidelizacion" or "Promociones" or "ReportesComercial"
                => "🛒 Comercial - Ventas y Clientes",
            
            // 🍽️ Contexto Operaciones
            "Comandas" or "Reservaciones" or "Mesas" or "Preparaciones" or "ReportesOperaciones"
                => "🍽️ Operaciones - Restaurante",
            
            // 📦 Contexto Inventario
            "Ingredientes" or "OrdenesCompra" or "MovimientosInventario" or "ReportesInventario"
                => "📦 Inventario - Gestión de Stock",
            
            // 🏢 Contexto Proveedores
            "Proveedores" or "ContactosProveedor" or "EvaluacionesProveedor"
                => "🏢 Proveedores - Gestión Externa",
            
            // 🔐 Autenticación
            "Auth" => "🔐 Autenticación y Autorización",
            
            // 🛠️ Otros
            _ => "🛠️ Otros"
        };
    }

    /// <summary>
    /// Configura tags personalizados para mejor organización
    /// </summary>
    private static void ConfigureCustomTags(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
    {
        // Configurar descripciones detalladas para cada contexto
        var tags = new[]
        {
            new OpenApiTag
            {
                Name = "🏗️ Core - Funcionalidades Centrales",
                Description = "Gestión de productos, usuarios, notificaciones y recetas del sistema"
            },
            new OpenApiTag
            {
                Name = "🛒 Comercial - Ventas y Clientes", 
                Description = "Gestión de clientes, facturación, fidelización y promociones"
            },
            new OpenApiTag
            {
                Name = "🍽️ Operaciones - Restaurante",
                Description = "Gestión de comandas, reservaciones, mesas y preparaciones"
            },
            new OpenApiTag
            {
                Name = "📦 Inventario - Gestión de Stock",
                Description = "Control de ingredientes, órdenes de compra y movimientos"
            },
            new OpenApiTag
            {
                Name = "🏢 Proveedores - Gestión Externa",
                Description = "Gestión de proveedores, contactos y evaluaciones"
            },
            new OpenApiTag
            {
                Name = "🔐 Autenticación y Autorización",
                Description = "Endpoints de login, registro y gestión de accesos"
            }
        };

        // Los tags se configuran automáticamente por el TagActionsBy
    }
} 