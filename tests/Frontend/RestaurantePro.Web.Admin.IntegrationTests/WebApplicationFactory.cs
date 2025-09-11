using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System.Data.Common;

namespace RestaurantePro.Web.Admin.IntegrationTests;

/// <summary>
/// Factory para crear instancias de la API para pruebas de integración con base de datos en memoria
/// </summary>
public class WebApplicationFactory : WebApplicationFactory<RestaurantePro.Api.Program>, IAsyncLifetime
{
    private DbConnection? _connection;

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

            // Agregar base de datos en memoria
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase("RestaurantePro_IntegrationTests");
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

            // Configurar DbContext genérico para repositorios
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<RestauranteProDbContext>());

            // Configurar repositorios para pruebas
            services.AddScoped<RestaurantePro.Domain.Comercial.Clientes.Interfaces.IClienteRepository, 
                RestaurantePro.Infrastructure.Persistence.Repositories.Comercial.ClienteRepository>();

            // Configurar logging para pruebas
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
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
