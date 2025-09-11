using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Data.Common;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using RestaurantePro.Web.Admin.IntegrationTests.Services;

namespace RestaurantePro.Web.Admin.IntegrationTests.Core;

/// <summary>
/// Factory para crear instancias de la API para pruebas de integración con base de datos en memoria
/// </summary>
public class WebApplicationFactory : WebApplicationFactory<RestaurantePro.Api.Program>, IAsyncLifetime
{
    private DbConnection? _connection;
    private string _databaseName = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover la base de datos real
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Agregar base de datos en memoria con nombre único para cada prueba
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });

            // Configurar autenticación para pruebas
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", options => { });

            // Configurar autorización para pruebas
            services.AddAuthorization(options =>
            {
                options.AddPolicy("TestPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            // Configurar MediatR para pruebas
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(typeof(RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente.CrearClienteCommand).Assembly);
            });

            // Configurar AutoMapper para pruebas
            services.AddAutoMapper(typeof(RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente.CrearClienteCommand).Assembly);
            
            // Configurar INotificationService para pruebas
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.INotificationService, TestNotificationService>();
            
            // Configurar IEmailService para pruebas
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IEmailService, TestEmailService>();

            // Configurar serialización JSON para pruebas (enums como strings)
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // Configurar DbContext genérico para repositorios
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<RestauranteProDbContext>());

            // Configurar UnitOfWork para pruebas
            services.AddScoped<RestaurantePro.Domain.Core.SharedKernel.Interfaces.IUnitOfWork, 
                RestaurantePro.Infrastructure.Persistence.Repositories.Base.UnitOfWork>();

            // Configurar repositorios para pruebas
            services.AddScoped<RestaurantePro.Domain.Comercial.Clientes.Interfaces.IClienteRepository, 
                RestaurantePro.Infrastructure.Persistence.Repositories.Comercial.ClienteRepository>();
            
            services.AddScoped<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoRepository, 
                RestaurantePro.Infrastructure.Persistence.Repositories.Core.ProductoRepository>();
            
            services.AddScoped<RestaurantePro.Domain.Core.Productos.Interfaces.IProductoCategoriaRepository, 
                RestaurantePro.Infrastructure.Persistence.Repositories.Core.ProductoCategoriaRepository>();
            
            // Configurar servicios adicionales para pruebas
            services.AddScoped<RestaurantePro.Domain.Core.SharedKernel.Services.Cache.ICacheService, 
                TestCacheService>();

            // Configurar servicio de sanitización HTML para pruebas
            services.AddScoped<RestaurantePro.Application.Common.Services.IHtmlSanitizerService, 
                RestaurantePro.Application.Common.Services.HtmlSanitizerService>();

            // Configurar servicio de fecha y hora para pruebas
            services.AddScoped<RestaurantePro.Domain.Core.Base.Services.IDateTimeService, 
                RestaurantePro.Domain.Core.Base.Services.DateTimeService>();

            // Configurar política de visibilidad de categorías para pruebas
            services.AddScoped<RestaurantePro.Domain.Core.Productos.Policies.IVisibilidadCategoriasPolicy, 
                RestaurantePro.Domain.Core.Productos.Policies.VisibilidadCategoriasPolicy>();

            // Configurar logging para pruebas
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Generar nombre único para la base de datos de esta instancia
        _databaseName = $"RestaurantePro_IntegrationTests_{Guid.NewGuid():N}";
        
        // Crear la base de datos en memoria
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        await base.DisposeAsync();
    }
}
