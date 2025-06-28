using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Common.Models;
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
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.SharedKernel;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Infrastructure.DependencyInjection;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Factory personalizada para configurar la aplicación web en los tests de integración.
/// Sobrescribe la configuración de producción para usar base de datos en memoria.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // 🔧 CONFIGURACIÓN ROBUSTA DE SQLITE IN-MEMORY CON MEJORAS PARA CONCURRENCIA
    private static SqliteConnection? _connection;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();
    private static readonly string _databaseName = $"TestDatabase_{Guid.NewGuid():N}"; // Nombre único por ejecución

    public string DatabaseName => _databaseName;
    
    /// <summary>
    /// Elimina un servicio del contenedor de dependencias
    /// </summary>
    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 🔧 CONFIGURAR MODO TESTING PARA EVITAR CONFLICTOS DE BD
        Environment.SetEnvironmentVariable("TESTING_MODE", "true");
        
        builder.UseEnvironment("Development");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Configuración específica para testing con mejoras de concurrencia
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", $"Data Source={_databaseName};Mode=Memory;Cache=Shared"},
                {"Logging:LogLevel:Default", "Critical"}, // Solo errores críticos
                {"Logging:LogLevel:Microsoft", "Critical"},
                {"Logging:LogLevel:Microsoft.Hosting.Lifetime", "Critical"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Critical"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Critical"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Connection", "Critical"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Update", "Critical"},
                {"Logging:LogLevel:RestaurantePro.Application.Common.Behaviors", "Critical"},
                {"Logging:LogLevel:RestaurantePro.Infrastructure.Persistence", "Critical"},
                {"TESTING_MODE", "true"}
            });
        });

        // 🔧 CONFIGURACIÓN SIMPLIFICADA PARA TESTS
        builder.ConfigureServices((context, services) =>
        {
            // Obtener la configuración del contexto
            var configuration = context.Configuration;
            
            // 🔧 INICIALIZACIÓN ROBUSTA DE LA BASE DE DATOS SQLITE CON MEJORAS
            lock (_lock)
            {
                if (_connection == null)
                {
                    // 🔧 CONFIGURACIÓN SQLITE SIMPLIFICADA Y COMPATIBLE
                    var connectionString = $"Data Source={_databaseName};Mode=Memory;Cache=Shared";
                    _connection = new SqliteConnection(connectionString);
                    _connection.Open();
                    
                    // 🔧 CONFIGURAR SQLITE PARA MEJOR RENDIMIENTO EN TESTS (solo parámetros compatibles)
                    using var command = _connection.CreateCommand();
                    command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA synchronous=NORMAL; PRAGMA temp_store=MEMORY;";
                    command.ExecuteNonQuery();
                }

                // 🔧 CREAR ESQUEMA UNA SOLA VEZ CON MIGRACIONES
                if (!_databaseInitialized)
                {
                    using var dbContext = new RestauranteProDbContext(
                        new DbContextOptionsBuilder<RestauranteProDbContext>()
                            .UseSqlite(_connection)
                            .EnableSensitiveDataLogging(false) // Deshabilitar en tests para mejor rendimiento
                            .EnableDetailedErrors(false) // Deshabilitar en tests para mejor rendimiento
                            .Options,
                        new TestLogger<RestauranteProDbContext>(),
                        new TestDomainEventDispatcher());
                    
                    // 🔧 GARANTIZAR QUE LA BASE DE DATOS Y TABLAS SE CREEN CORRECTAMENTE
                    try
                    {
                        // Aplicar migraciones para crear todas las tablas con sus configuraciones completas
                        dbContext.Database.Migrate();
                        // Verificar que las tablas principales existen
                        var tables = dbContext.Database.SqlQueryRaw<string>(
                            "SELECT name FROM sqlite_master WHERE type='table'").ToList();
                        // Verificar que las tablas críticas existen
                        var criticalTables = new[] { "Usuarios", "Productos", "Clientes", "Mesas", "Comandas", "Facturas", "OrdenesCompra" };
                        var missingTables = criticalTables.Where(table => !tables.Contains(table)).ToList();
                        if (missingTables.Any())
                        {
                            throw new Exception($"Faltan tablas críticas en la base de datos de test: {string.Join(", ", missingTables)}. Revisa las migraciones.");
                        }
                        _databaseInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        // Fallback: recrear la base de datos con migraciones
                        dbContext.Database.EnsureDeleted();
                        dbContext.Database.EnsureCreated();
                        _databaseInitialized = true;
                    }
                }
            }

            // 🔧 CONFIGURAR SQLITE EN MEMORIA CON CONEXIÓN ESTÁTICA Y OPTIMIZACIONES
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging(false); // Deshabilitar en tests
                options.EnableDetailedErrors(false); // Deshabilitar en tests
                options.ConfigureWarnings(warnings => warnings
                    .Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.NavigationBaseIncludeIgnored)
                    .Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)); // Suprimir warning de cambios pendientes
            });

            // 🔧 REGISTRAR IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // 🔧 REGISTRAR DbContext GENÉRICO PARA REPOSITORIOS
            services.AddScoped<DbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // REGISTRO GLOBAL DE INFRAESTRUCTURA PARA TESTS
            services.AddInfrastructureServices(configuration, isTestEnvironment: true);

            // 🔧 CONFIGURAR AUTENTICACIÓN PARA TESTS
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Test")
                    .Build();
            });

            // 🔧 CONFIGURAR LOGGING PARA TESTS (SUPRIMIR WARNINGS ESPERADOS)
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Critical); // Solo errores críticos
                // Configurar filtros específicos para tests
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Critical);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Connection", LogLevel.Critical);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.Critical);
                builder.AddFilter("Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware", LogLevel.Critical);
                builder.AddFilter("RestaurantePro.Application.Common.Behaviors", LogLevel.Critical);
                builder.AddFilter("RestaurantePro.Infrastructure.Persistence", LogLevel.Critical);
                builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Critical);
            });

            // 🔧 REGISTRAR SERVICIO FAKE DE FECHA/HORA
            services.AddSingleton<IDateTimeService, FakeDateTimeService>();
            services.AddSingleton<ITimeProvider, FakeTimeProvider>();
            services.AddSingleton<IDelayProvider, FakeDelayProvider>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // No cerrar la conexión aquí, solo limpiar datos si es necesario
        }
        base.Dispose(disposing);
    }

    /// <summary>
    /// Limpia todos los datos de la base de datos sin cerrar la conexión
    /// </summary>
    public static void CleanupDatabase()
    {
        if (_connection != null && _databaseInitialized)
        {
            using var context = new RestauranteProDbContext(
                new DbContextOptionsBuilder<RestauranteProDbContext>()
                    .UseSqlite(_connection)
                    .Options,
                new TestLogger<RestauranteProDbContext>(),
                new TestDomainEventDispatcher());
            
            // 🔧 LIMPIEZA SEGURA: SOLO ELIMINAR DATOS, NO RECREAR ESQUEMA
            try
            {
                // Desactivar detección de cambios para mejorar rendimiento
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                
                // Limpiar datos en orden específico para evitar problemas de FK
                context.Facturas.RemoveRange(context.Facturas);
                context.ItemsComanda.RemoveRange(context.ItemsComanda);
                context.Comandas.RemoveRange(context.Comandas);
                context.Reservaciones.RemoveRange(context.Reservaciones);
                context.Mesas.RemoveRange(context.Mesas);
                context.Clientes.RemoveRange(context.Clientes);
                context.Usuarios.RemoveRange(context.Usuarios);
                context.Productos.RemoveRange(context.Productos);
                context.Ingredientes.RemoveRange(context.Ingredientes);
                context.Proveedores.RemoveRange(context.Proveedores);
                context.Notificaciones.RemoveRange(context.Notificaciones);
                
                // Guardar cambios
                context.SaveChanges();
                
                // Reactivar detección de cambios
                context.ChangeTracker.AutoDetectChangesEnabled = true;
                
                Console.WriteLine("✅ Datos de la base de datos limpiados correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error limpiando datos: {ex.Message}");
                // Fallback: recrear solo si es absolutamente necesario
                context.Database.EnsureCreated();
            }
        }
    }

    /// <summary>
    /// Cierra la conexión SQLite estática (llamar solo al final de todos los tests)
    /// </summary>
    public static void CloseConnection()
    {
        lock (_lock)
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
                _databaseInitialized = false;
            }
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
    // Esta colección ejecuta tests secuencialmente para evitar conflictos de BD
}

[CollectionDefinition("Parallel")]
public class ParallelCollection : ICollectionFixture<TestWebApplicationFactory>
{
    // Esta colección permite ejecución paralela para tests independientes
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestWebApplicationFactory>
{
    // Esta colección agrupa tests que requieren acceso a BD
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

/// <summary>
/// Logger de test para DbContext
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

/// <summary>
/// Dispatcher de eventos de dominio para tests
/// </summary>
public class TestDomainEventDispatcher : IDomainEventDispatcher
{
    public Task Dispatch(DomainEvent evento, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DispatchAll(IEnumerable<DomainEvent> eventos, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

// Implementación fake para IDateTimeService
public class FakeDateTimeService : IDateTimeService
{
    private DateTime _now = DateTime.UtcNow;
    public DateTime Now => _now;
    public DateTime Today => _now.Date;
    public DateTime UtcNow => _now.ToUniversalTime();
    public void SetNow(DateTime now) => _now = now;
}

// Implementación fake para ITimeProvider
public class FakeTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    
    public long GetTimestamp()
    {
        return DateTimeOffset.UtcNow.Ticks;
    }
    
    public TimeSpan GetElapsedTime(long startingTimestamp)
    {
        var currentTicks = DateTimeOffset.UtcNow.Ticks;
        var elapsedTicks = currentTicks - startingTimestamp;
        return TimeSpan.FromTicks(elapsedTicks);
    }
}

// Implementación fake para IDelayProvider
public class FakeDelayProvider : IDelayProvider
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No delay en tests
    }
    
    public Task DelayAsync(int millisecondsDelay, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No delay en tests
    }
    
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask; // No delay en tests
    }
}

// 🔧 SERVICIO DE NOTIFICACIÓN PARA TESTS
public class TestNotificationService : INotificationService
{
    private readonly ILogger<TestNotificationService> _logger;

    public TestNotificationService(ILogger<TestNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EnviarNotificacionAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("📧 TestNotificationService: Enviando notificación a {UsuarioId}: {Titulo} - {Mensaje}", usuarioId, titulo, mensaje);
        return await Task.FromResult(true);
    }

    public async Task<bool> EnviarNotificacionMasivaAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("📧 TestNotificationService: Enviando notificación masiva a {Count} usuarios: {Titulo} - {Mensaje}", usuariosIds.Count, titulo, mensaje);
        return await Task.FromResult(true);
    }

    public async Task<bool> EnviarNotificacionPushAsync(Guid usuarioId, string titulo, string mensaje)
    {
        _logger.LogInformation("📲 TestNotificationService: Enviando notificación push a {UsuarioId}: {Titulo} - {Mensaje}", usuarioId, titulo, mensaje);
        return await Task.FromResult(true);
    }

    public async Task<bool> MarcarComoLeidaAsync(Guid notificacionId, Guid usuarioId)
    {
        _logger.LogInformation("✅ TestNotificationService: Marcando notificación {NotificacionId} como leída para usuario {UsuarioId}", notificacionId, usuarioId);
        return await Task.FromResult(true);
    }

    public async Task<int> ObtenerNotificacionesNoLeidasAsync(Guid usuarioId)
    {
        _logger.LogInformation("📊 TestNotificationService: Obteniendo notificaciones no leídas para usuario {UsuarioId}", usuarioId);
        return await Task.FromResult(0); // Siempre retorna 0 en tests
    }

    public async Task<bool> SendNotificationAsync(RestaurantePro.Application.Common.Models.Notification notification)
    {
        _logger.LogInformation("📧 TestNotificationService: Enviando notificación: {Title} - {Message}", notification.Title, notification.Message);
        return await Task.FromResult(true);
    }
}