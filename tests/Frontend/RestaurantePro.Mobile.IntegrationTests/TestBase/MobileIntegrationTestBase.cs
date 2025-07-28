using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantePro.Api;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Infrastructure.Identity;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR;
using AutoMapper;
using RestaurantePro.Application.Config.DependencyInjection;

namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

/// <summary>
/// Fixture compartido para tests de integración móvil
/// Se ejecuta una sola vez por clase de test para evitar duplicados
/// </summary>
public class MobileIntegrationTestFixture : WebApplicationFactory<Program>, IDisposable
{
    private bool _disposed = false;
    private static bool _seedExecuted = false;
    private static readonly object _seedLock = new object();

    public MobileIntegrationTestFixture()
    {
        // 🔧 EJECUTAR SEED DATA UNA SOLA VEZ GLOBALMENTE
        lock (_seedLock)
        {
            if (!_seedExecuted)
            {
                Console.WriteLine("🔧 EJECUTANDO SEED DE DATOS GLOBAL...");
                using var scope = Services.CreateScope();
                var seedService = scope.ServiceProvider.GetRequiredService<ISeedDataService>();
                seedService.SeedAsync().Wait();
                _seedExecuted = true;
                Console.WriteLine("✅ Seed de datos global completado");
            }
            else
            {
                Console.WriteLine("ℹ️ Seed de datos ya ejecutado, saltando...");
            }
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 🔧 CONFIGURAR VARIABLES DE ENTORNO PARA TESTS
        Environment.SetEnvironmentVariable("TESTING_MODE", "true");
        Environment.SetEnvironmentVariable("UseInMemoryDatabase", "true");
        
        // 🔧 CONFIGURAR CONFIGURACIÓN DE TESTS
        var testConfig = new Dictionary<string, string?>
        {
            {"UseInMemoryDatabase", "true"},
            {"ConnectionStrings:DefaultConnection", "Data Source=:memory:"},
            {"JwtSettings:Secret", "SuperSecretKeyForTestingPurposesOnly123456789"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationInMinutes", "60"},
            {"JwtSettings:RefreshTokenExpirationInDays", "7"}
        };

        builder.UseConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(testConfig)
            .Build());

        // 🔧 CONFIGURAR SERVICIOS PARA TESTS
        builder.ConfigureServices(services =>
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            // ⚠️ Reemplazar la base de datos por InMemory (patrón estándar)
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }
            
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
            });

            // 🔧 REGISTRAR SERVICIOS DE INFRAESTRUCTURA
            services.AddPersistenceServices(configuration, isTestEnvironment: true);
            IdentitySetup.AddIdentityServices(services, configuration);
            
            // 🔧 REGISTRAR SERVICIOS DE IDENTIDAD PARA AUTENTICACIÓN
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IIdentityService, RestaurantePro.Infrastructure.Identity.Services.IdentityService>();
            
            // 🔧 REGISTRAR MEDIATR CON TODOS LOS ASSEMBLIES NECESARIOS (UNA SOLA VEZ)
            services.AddMediatR(typeof(RestaurantePro.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados.ObtenerClientesPaginadosQuery).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados.ObtenerProductosPaginadosQuery).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas.ObtenerMesasQuery).Assembly);
            
            // 🔧 REGISTRAR AUTOMAPPER PARA LOS HANDLERS
            services.AddAutoMapper(typeof(RestaurantePro.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
            
            // 🔧 REGISTRAR SERVICIOS MOCK
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IEmailService, MockEmailService>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.ISignalRService, MockSignalRService>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IDelayProvider, MockDelayProvider>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IFileStorageService, MockFileStorageService>();
            
            // 🔧 REGISTRAR EL SERVICIO DE SEED DE DATOS
            services.AddScoped<ISeedDataService, TestSeedDataService>();
            
            // 🔧 REGISTRAR SERVICIOS MOCK ADICIONALES PARA EVENT HANDLERS
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.ISMSService, MockSMSService>();
            
            // 🔧 REGISTRAR CONTROLADORES ESPECÍFICOS MANUALMENTE
            services.AddControllers()
                    .AddApplicationPart(typeof(RestaurantePro.Api.Controllers.Core.AuthController).Assembly);
            
            // ✅ AHORA LOS CONTROLADORES DEBERÍAN FUNCIONAR CORRECTAMENTE
        });
        
        // 🔧 CONFIGURAR EL PIPELINE HTTP COMPLETO PARA TESTS
        builder.Configure(app =>
        {
            // 🔧 FORZAR EL DESCUBRIMIENTO DE CONTROLADORES
            Console.WriteLine("🔧 CONFIGURANDO PIPELINE HTTP...");
            
            // 🔧 CONFIGURAR EL PIPELINE HTTP COMPLETO
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                // 🔧 MAPEAR CONTROLADORES (CRÍTICO PARA QUE FUNCIONEN LOS ENDPOINTS)
                endpoints.MapControllers();
                Console.WriteLine("✅ Controladores mapeados");
                
                // 🔧 VERIFICAR QUE LOS CONTROLADORES SE REGISTRARON CORRECTAMENTE
                var controllerActions = endpoints.DataSources
                    .SelectMany(ds => ds.Endpoints)
                    .OfType<RouteEndpoint>()
                    .Where(e => e.DisplayName?.Contains("Controller") == true)
                    .Select(e => e.DisplayName)
                    .ToList();
                
                Console.WriteLine($"🔧 Controladores registrados: {string.Join(", ", controllerActions)}");
            });
            
            // ⚠️ NO ejecutar seed aquí, ya se ejecuta en el constructor del fixture
        });
        
        // 🔧 FORZAR EL DESCUBRIMIENTO DEL ASSEMBLY
        builder.UseSetting(Microsoft.AspNetCore.Hosting.WebHostDefaults.ApplicationKey, typeof(Program).Assembly.FullName);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 🔧 LIMPIAR RECURSOS DEL FIXTURE
                base.Dispose(disposing);
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Clase base para tests de integración móvil que usa el fixture compartido
/// </summary>
public abstract class MobileIntegrationTestBase : IClassFixture<MobileIntegrationTestFixture>
{
    protected readonly MobileIntegrationTestFixture _fixture;
    protected readonly HttpClient _client;

    protected MobileIntegrationTestBase(MobileIntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    protected HttpClient CreateClient() => _fixture.CreateClient();
}

/// <summary>
/// Configuración de JWT para tests
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
} 