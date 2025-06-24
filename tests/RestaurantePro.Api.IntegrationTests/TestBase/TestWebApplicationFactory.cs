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
using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Core.Notificaciones.Services;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Services;
using Microsoft.Data.Sqlite;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Comercial.Services;
using Microsoft.AspNetCore.Http;

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

            // 🔧 REGISTRAR DBCONTEXT GENÉRICO PARA REPOSITORIOS
            services.AddScoped<DbContext>(provider => 
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

            // 🔧 REGISTRAR IHttpContextAccessor PARA TestCurrentUserService
            services.AddHttpContextAccessor();

            // 🔧 REGISTRAR SERVICIOS BÁSICOS QUE APPLICATION NECESITA
            // ITimeProvider - necesario para PerformanceBehavior
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            
            // IDelayProvider - necesario para RetryBehavior
            services.AddSingleton<IDelayProvider, DelayProvider>();
            
            // IDateTimeService - necesario para PreparacionRepository
            services.AddScoped<IDateTimeService>(provider => 
                new MockDateTimeService(DateTime.Now)); // Usar fecha actual para tests
            
            // 🔔 INotificationService - necesario para Commands como DesactivarCliente
            services.AddScoped<INotificationService, NotificationService>();
            
            // 📧 IEmailService - Mock para evitar errores en tests
            services.AddScoped<IEmailService, TestEmailService>();
            
            // 👤 ICurrentUserService - necesario para CrearUsuarioHandler
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            
            // 📦 REGISTRAR REPOSITORIOS NECESARIOS PARA LOS TESTS
            services.AddScoped<IProductoRepository, ProductoRepository>();
            
            // 📦 REGISTRAR REPOSITORIOS CORE
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            
            // 📦 REGISTRAR REPOSITORIOS COMERCIAL
            services.AddScoped<IClienteRepository, ClienteRepository>();
            
            // 📦 REGISTRAR REPOSITORIOS DE FACTURACIÓN
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            services.AddScoped<IHistorialPuntosRepository, HistorialPuntosRepository>();
            
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
            
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            
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

            // Crear el esquema de la base de datos en memoria
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
                db.Database.EnsureCreated(); // O usa db.Database.Migrate() si usas migraciones
            }

            // Repositorios Base
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Servicios de dominio y utilidades
            services.AddScoped<IDateTimeService, DateTimeService>();

            // Fakes para servicios de identidad y permisos
            services.AddScoped<IIdentityService, FakeIdentityService>();
            services.AddScoped<IJwtTokenService, FakeJwtTokenService>();
            services.AddScoped<IUserPermissionService, FakeUserPermissionService>();

            // 📦 REGISTRAR SERVICIOS DE FACTURACIÓN
            services.AddScoped<IServicioFacturacion, ServicioFacturacion>();
            services.AddScoped<IComercialServiceFacade, ComercialServiceFacade>();
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
        // Verificar si hay un header de autorización
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine($"🔍 TestAuthenticationHandler: Header recibido: {authHeader}");
        
        if (!authHeader.StartsWith("Test "))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authentication scheme"));
        }

        // Extraer el rol del header si está presente
        var role = "Administrador"; // Rol por defecto
        var userId = Guid.NewGuid().ToString(); // ID por defecto
        
        // Remover el prefijo "Test " para procesar el resto
        var authValue = authHeader.Substring(5); // "Test " tiene 5 caracteres
        Console.WriteLine($"🔍 TestAuthenticationHandler: Valor de autorización: {authValue}");
        
        if (authValue.StartsWith("User_"))
        {
            // Formato: "User_{userId}_{rol}"
            var userPart = authValue.Substring(5); // Remover "User_"
            var parts = userPart.Split('_');
            Console.WriteLine($"🔍 TestAuthenticationHandler: Parts encontrados: {string.Join(", ", parts)}");
            
            if (parts.Length >= 2)
            {
                if (Guid.TryParse(parts[0], out var parsedUserId))
                {
                    userId = parsedUserId.ToString();
                    Console.WriteLine($"🔍 TestAuthenticationHandler: UserId parseado correctamente: {userId}");
                }
                else
                {
                    Console.WriteLine($"🔍 TestAuthenticationHandler: Error al parsear UserId: {parts[0]}");
                }
                role = parts[1];
            }
        }
        else if (authValue.StartsWith("AuthenticatedUser-"))
        {
            // Formato: "AuthenticatedUser-{rol}"
            role = authValue.Substring(17); // Remover "AuthenticatedUser-"
            Console.WriteLine($"🔍 TestAuthenticationHandler: Formato AuthenticatedUser detectado, rol: {role}");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Role, role)
        };

        // Agregar múltiples roles para cubrir todos los endpoints
        var allRoles = new[] { "Administrador", "Cajero", "Gerente", "Mesero", "Cocinero" };
        foreach (var r in allRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, r));
        }

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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId 
    {
        get
        {
            // Intentar obtener el ID del usuario del contexto HTTP
            var user = _httpContextAccessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(nameIdentifier))
                {
                    Console.WriteLine($"🔍 TestCurrentUserService: Obtenido UserId del contexto HTTP: {nameIdentifier}");
                    return nameIdentifier;
                }
            }
            
            // Fallback al ID fijo si no se puede obtener del contexto
            Console.WriteLine($"🔍 TestCurrentUserService: Usando UserId fallback: 12345678-1234-1234-1234-123456789012");
            return "12345678-1234-1234-1234-123456789012";
        }
    }
    
    public string? UserName => "TestUser";
    public string? Email => "test@test.com";
    public bool IsAuthenticated => true;
    public IEnumerable<string> Roles => new[] { "Administrador", "Cajero", "Gerente" };
    public string? Rol => "Administrador";

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Mock del servicio de email para tests de integración
/// Evita errores de dirección nula y no envía correos reales
/// </summary>
public class TestEmailService : IEmailService
{
    private readonly ILogger<TestEmailService> _logger;

    public TestEmailService(ILogger<TestEmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        // En tests, solo loguear que se intentó enviar el email
        _logger.LogInformation("📧 [TEST] Email simulado enviado a {To} con asunto: {Subject}", to, subject);
        
        // Simular un pequeño delay para hacer el método async
        await Task.Delay(1);
        
        // Siempre retornar true para simular éxito
        return true;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null)
    {
        // En tests, solo loguear que se intentó enviar el email con adjunto
        _logger.LogInformation("📧 [TEST] Email con adjunto simulado enviado a {To} con asunto: {Subject}", to, subject);
        
        // Simular un pequeño delay para hacer el método async
        await Task.Delay(1);
        
        // Siempre retornar true para simular éxito
        return true;
    }

    public async Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        // En tests, solo loguear que se intentó enviar el email HTML
        _logger.LogInformation("📧 [TEST] Email HTML simulado enviado a {To} con asunto: {Subject}", to, subject);
        
        // Simular un pequeño delay para hacer el método async
        await Task.Delay(1);
        
        // Siempre retornar true para simular éxito
        return true;
    }

    public async Task<bool> SendBulkEmailAsync(List<string> toAddresses, string subject, string body)
    {
        // En tests, solo loguear que se intentó enviar el email masivo
        _logger.LogInformation("📧 [TEST] Email masivo simulado enviado a {Count} destinatarios con asunto: {Subject}", toAddresses.Count, subject);
        
        // Simular un pequeño delay para hacer el método async
        await Task.Delay(1);
        
        // Siempre retornar true para simular éxito
        return true;
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        // En tests, solo loguear que se intentó enviar el email con adjunto
        _logger.LogInformation("📧 [TEST] Email con adjunto simulado enviado a {To} con asunto: {Subject}", to, subject);
        
        // Simular un pequeño delay para hacer el método async
        await Task.Delay(1);
        
        // Siempre retornar true para simular éxito
        return true;
    }
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

// Fake para IIdentityService
public class FakeIdentityService : IIdentityService
{
    public Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
        => Task.FromResult((Result.Success(), "fake-user-id"));
    
    public Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
        => Task.FromResult(Result.Success("fake-user-id"));
    
    public Task<Result> CreateRoleAsync(string roleName, string description, bool isSystemRole)
        => Task.FromResult(Result.Success());
    
    public Task<AuthResponse> LoginAsync(string email, string password)
        => Task.FromResult(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } });
    
    public Task<List<UserDto>> GetUsersAsync()
        => Task.FromResult(new List<UserDto>());
    
    public Task<UserDto> GetUserByIdAsync(string userId)
        => Task.FromResult(new UserDto { Id = userId, UserName = "FakeUser", Email = "fake@email.com", EmailConfirmed = true, Roles = new List<string> { "Admin" } });
    
    public Task<Result> UpdateUserAsync(string id, string nombre, string apellidos, string email, string username)
        => Task.FromResult(Result.Success());
    
    public Task<Result> DeleteUserAsync(string userId)
        => Task.FromResult(Result.Success());
    
    public Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        => Task.FromResult(Result.Success());
    
    public Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
        => Task.FromResult(Result.Success(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } }));
    
    public Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        => Task.FromResult(Result.Success(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } }));
}

// Fake para IJwtTokenService
public class FakeJwtTokenService : IJwtTokenService
{
    public JwtTokenResponse GenerateToken(string userId, string userName, string email, IList<string> roles)
        => new JwtTokenResponse { AccessToken = "fake-jwt-token", TokenType = "Bearer", ExpiresIn = 3600, RequiresRefresh = false };
    
    public string GenerateRefreshToken() => "fake-refresh-token";
    
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token) => new ClaimsPrincipal();
}

// Fake para IUserPermissionService
public class FakeUserPermissionService : IUserPermissionService
{
    public Task<bool> UsuarioTienePermisoAsync(Guid usuarioId, string permiso) => Task.FromResult(true);
    public Task<bool> UsuarioTieneRolAsync(Guid usuarioId, string rol) => Task.FromResult(true);
    public Task<bool> UsuarioTieneNivelAccesoAsync(Guid usuarioId, int nivelRequerido) => Task.FromResult(true);
    public Task<List<string>> ObtenerPermisosUsuarioAsync(Guid usuarioId) => Task.FromResult(new List<string>());
    public Task<List<Guid>> ObtenerSubordinadosAsync(Guid supervisorId) => Task.FromResult(new List<Guid>());
    public Task<bool> PuedeSupervisarAsync(Guid supervisorId, Guid subordinadoId) => Task.FromResult(true);
    public Task<List<Guid>> ObtenerUsuariosMismoDepartamentoAsync(Guid usuarioId) => Task.FromResult(new List<Guid>());
    public Task<bool> EsAdministradorAsync(Guid usuarioId) => Task.FromResult(true);
}