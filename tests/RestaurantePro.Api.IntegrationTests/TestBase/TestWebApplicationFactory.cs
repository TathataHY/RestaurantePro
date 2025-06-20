using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Infrastructure.Services;
using RestaurantePro.Infrastructure.ExternalServices.Email;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Core.Productos.Builders;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Factory personalizada para configurar la aplicación web en los tests de integración.
/// Sobrescribe la configuración de producción para usar base de datos en memoria.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // 🔧 BD única por test para evitar contaminación de datos
    private readonly string _databaseName = $"TestDatabase_{Guid.NewGuid()}";
    
    public string DatabaseName => _databaseName;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuraciones existentes
            config.Sources.Clear();
            
            // Agregar configuración específica para tests que FUERZA InMemory
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "InMemoryDatabase",
                ["UseInMemoryDatabase"] = "true",  // Flag para Infrastructure
                ["Logging:LogLevel:Default"] = "Error",      // Solo errores en tests
                ["Logging:LogLevel:Microsoft"] = "Error",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Error"
            });
            
            // Configurar variable para modo testing
            Environment.SetEnvironmentVariable("TESTING_MODE", "true");
            
            // Agregar variables de entorno para tests
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // 🚀 CONFIGURACIÓN PARA TESTS: Infrastructure está desactivada por Program.cs
            // Necesitamos registrar servicios mínimos necesarios
            
            // 🔧 SOLUCIÓN: BD única por test para total aislamiento
            // Cada instancia del factory usa una BD InMemory diferente
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Registrar IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // 🔐 CONFIGURACIÓN DE AUTENTICACIÓN FAKE PARA TESTS
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "Test", options => { });
            
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Test")
                    .Build();
            });

            // 🔧 REGISTRAR SERVICIOS BÁSICOS QUE APPLICATION NECESITA
            // ITimeProvider - necesario para PerformanceBehavior
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            
            // IDelayProvider - necesario para RetryBehavior
            services.AddSingleton<IDelayProvider, DelayProvider>();
            
            // 🔔 INotificationService - necesario para Commands como DesactivarCliente
            services.AddScoped<INotificationService, NotificationService>();
            
            // 📧 IEmailService - necesario para Commands como DesactivarCliente
            services.AddScoped<IEmailService, EmailService>();
            
            // 👤 ICurrentUserService - necesario para CrearUsuarioHandler
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            
            // 📦 REGISTRAR REPOSITORIOS NECESARIOS PARA LOS TESTS
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            // 📦 REGISTRAR REPOSITORIOS COMERCIAL
            services.AddScoped<IClienteRepository>(provider => 
                new ClienteRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<ClienteRepository>>()));
            
            // 🏗️ REGISTRAR BUILDERS DE DOMAIN
            services.AddScoped<ProductoBuilder>();
            
            // Otros servicios básicos que podrían ser necesarios
            // services.AddSingleton<ICacheService, InMemoryCacheService>();
            // services.AddScoped<ICurrentUserService, TestCurrentUserService>();

            // Configurar logging mínimo para tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Error);
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Para InMemory database no necesitamos limpiar manualmente
            // ya que se desecha automáticamente al finalizar el test
        }
        
        base.Dispose(disposing);
    }
}

/// <summary>
/// Authentication handler personalizado para tests que bypasa la autenticación real
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Role, "Administrador") // Dar rol de Administrador para tests
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Implementación mock de ICurrentUserService para tests
/// </summary>
public class TestCurrentUserService : ICurrentUserService
{
    public string? UserId => "test-user-id";
    public string? UserName => "TestUser";
    public string? Email => "test@test.com";
    public bool IsAuthenticated => true;
    public IEnumerable<string> Roles => new[] { "Administrador" };
    public string? Rol => "Administrador";

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}