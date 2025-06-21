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
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.Base.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Services;
using Microsoft.Data.Sqlite;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Factory personalizada para configurar la aplicación web en los tests de integración.
/// Sobrescribe la configuración de producción para usar base de datos en memoria.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    // 🔧 BD única por test para evitar contaminación de datos
    private readonly string _databaseName = $"TestDatabase_{Guid.NewGuid()}";
    
    public string DatabaseName => _databaseName;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuraciones existentes
            config.Sources.Clear();
            
            // Agregar configuración específica para tests con SQLite in-memory
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                ["UseInMemoryDatabase"] = "false", // Usar SQLite en lugar de EF InMemory
                ["Logging:LogLevel:Default"] = "Error",      // Solo errores reales
                ["Logging:LogLevel:Microsoft"] = "Error",
                ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Error",
                ["Logging:LogLevel:RestaurantePro.Application.Common.Behaviors"] = "None", // Deshabilitar behaviors
                ["Logging:LogLevel:RestaurantePro.Application.Common.Exceptions.ValidationException"] = "None", // Deshabilitar validaciones
                ["Logging:Console:FormatterName"] = "json",
                ["Logging:Console:FormatterOptions:IncludeScopes"] = "true",
                ["Logging:Console:FormatterOptions:TimestampFormat"] = "yyyy-MM-dd HH:mm:ss "
            });
            
            // Configurar variable para modo testing
            Environment.SetEnvironmentVariable("TESTING_MODE", "true");
            
            // Agregar variables de entorno para tests
            config.AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            // Eliminar el registro previo de DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Crear y abrir la conexión SQLite in-memory
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseSqlite(_connection);
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
            
            // IDateTimeService - necesario para PreparacionRepository
            services.AddScoped<IDateTimeService, DateTimeService>();
            
            // 🔔 INotificationService - necesario para Commands como DesactivarCliente
            services.AddScoped<INotificationService, NotificationService>();
            
            // 📧 IEmailService - necesario para Commands como DesactivarCliente
            services.AddScoped<IEmailService, EmailService>();
            
            // 👤 ICurrentUserService - necesario para CrearUsuarioHandler
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            
            // 📦 REGISTRAR REPOSITORIOS NECESARIOS PARA LOS TESTS
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            // 📦 REGISTRAR REPOSITORIOS CORE
            services.AddScoped<IUsuarioRepository>(provider => 
                new UsuarioRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<UsuarioRepository>>()));
            
            // 📦 REGISTRAR REPOSITORIOS COMERCIAL
            services.AddScoped<IClienteRepository>(provider => 
                new ClienteRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<ClienteRepository>>()));
            
            // 📦 REGISTRAR REPOSITORIOS INVENTARIO
            services.AddScoped<IOrdenCompraRepository>(provider => 
                new OrdenCompraRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<OrdenCompraRepository>>()));
            
            services.AddScoped<IIngredienteRepository>(provider => 
                new IngredienteRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<IngredienteRepository>>()));
            
            services.AddScoped<IMovimientoInventarioRepository>(provider => 
                new MovimientoInventarioRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<MovimientoInventarioRepository>>()));
            
            services.AddScoped<IProveedorRepository>(provider => 
                new ProveedorRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<ProveedorRepository>>()));

            // 📦 REGISTRAR SERVICIOS DE DOMINIO NECESARIOS
            services.AddScoped<INotificationManager, NotificationManager>();
            services.AddScoped<IStockBajoPolicy, StockBajoPolicy>();
            services.AddScoped<IInventarioServiceFacade, InventarioServiceFacade>();
            services.AddScoped<IServicioNotificaciones, ServicioNotificaciones>();
            
            // 📦 REGISTRAR SERVICIOS DE USUARIOS
            services.AddScoped<IUsuarioService, UsuarioService>();
            
            services.AddScoped<INotificacionRepository>(provider => 
                new NotificacionRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<NotificacionRepository>>()));
            
            // 📦 REGISTRAR REPOSITORIOS OPERACIONES
            services.AddScoped<IPreparacionRepository>(provider => 
                new PreparacionRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<IDateTimeService>(),
                    provider.GetRequiredService<ILogger<PreparacionRepository>>()));
            
            services.AddScoped<IReservacionRepository>(provider => 
                new ReservacionRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<IMesaRepository>(),
                    provider.GetRequiredService<ILogger<ReservacionRepository>>()));
            
            services.AddScoped<IMesaRepository>(provider => 
                new MesaRepository(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<MesaRepository>>(),
                    provider.GetRequiredService<IDateTimeService>()));
            
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

            // Build el provider y aplicar migraciones
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
                db.Database.EnsureCreated();
                // db.Database.Migrate(); // Si tienes migraciones
            }
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
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

/// <summary>
/// Definición de la colección Sequential para tests de integración de API
/// Permite que xUnit inyecte correctamente el TestWebApplicationFactory
/// </summary>
[CollectionDefinition("Sequential")]
public class SequentialCollection : ICollectionFixture<TestWebApplicationFactory>
{
    // Esta clase no necesita implementación
    // Solo define la colección para xUnit
}