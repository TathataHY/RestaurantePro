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
using FluentValidation;
using RestaurantePro.Mobile.Core.Services.Notifications;
using RestaurantePro.Mobile.Core.Services.Realtime;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Linq;

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
        // 🔧 CONFIGURAR EL CONTENT ROOT CORRECTO PARA EL PROYECTO API
        // Buscar el directorio raíz del proyecto (donde está el .sln)
        var currentDir = Directory.GetCurrentDirectory();
        var solutionDir = currentDir;
        while (!File.Exists(Path.Combine(solutionDir, "RestaurantePro.sln")))
        {
            var parentDir = Directory.GetParent(solutionDir);
            if (parentDir == null) break;
            solutionDir = parentDir.FullName;
        }
        
        var apiProjectPath = Path.Combine(solutionDir, "src", "Backend", "RestaurantePro.Api");
        Console.WriteLine($"🔍 Directorio actual: {currentDir}");
        Console.WriteLine($"🔍 Directorio de solución: {solutionDir}");
        Console.WriteLine($"🔍 Ruta del proyecto API: {apiProjectPath}");
        Console.WriteLine($"🔍 ¿Existe el directorio? {Directory.Exists(apiProjectPath)}");
        builder.UseContentRoot(apiProjectPath);
        
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
            
            // 🔧 CONFIGURAR IDENTITY SIN JWT BEARER PARA TESTS (como en los tests del backend)
            TestIdentityConfiguration.ConfigureIdentityForTests(services, configuration);
            
            // 🔧 RE-REGISTRAR AUTENTICACIÓN PARA FORZAR EL HANDLER DE TEST COMO ESQUEMA POR DEFECTO
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            
            // 🔧 REGISTRAR SERVICIOS DE IDENTIDAD PARA AUTENTICACIÓN
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IIdentityService, RestaurantePro.Infrastructure.Identity.Services.IdentityService>();
            
            // 🔧 REGISTRAR SERVICIO DE SANITIZACIÓN HTML ANTES DE MEDIATR (REQUERIDO POR HANDLERS)
            services.AddScoped<RestaurantePro.Application.Common.Services.IHtmlSanitizerService, RestaurantePro.Application.Common.Services.HtmlSanitizerService>();
            
            // 🔧 REGISTRAR MEDIATR CON TODOS LOS ASSEMBLIES NECESARIOS (UNA SOLA VEZ)
            services.AddMediatR(typeof(RestaurantePro.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados.ObtenerClientesPaginadosQuery).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados.ObtenerProductosPaginadosQuery).Assembly);
            services.AddMediatR(typeof(RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesas.ObtenerMesasQuery).Assembly);
            
            // 🔧 REGISTRAR VALIDATORS
            services.AddValidatorsFromAssembly(typeof(RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados.ObtenerProductosPaginadosQuery).Assembly);
            services.AddValidatorsFromAssembly(typeof(RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados.ObtenerClientesPaginadosQuery).Assembly);
            
            // 🔧 REGISTRAR VALIDATION BEHAVIOR
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RestaurantePro.Application.Common.Behaviors.ValidationBehavior<,>));
            
            // 🔧 REGISTRAR AUTOMAPPER PARA LOS HANDLERS
            services.AddAutoMapper(typeof(RestaurantePro.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
            
            // 🔧 REGISTRAR SERVICIOS MOCK
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IEmailService, MockEmailService>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.ISignalRService, MockSignalRService>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IDelayProvider, MockDelayProvider>();
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.IFileStorageService, MockFileStorageService>();
            
            // 🔧 REGISTRAR API SERVICE PERSONALIZADO PARA TESTS
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Api.IApiService, TestApiService>();
            
            // 🔧 REGISTRAR EL SERVICIO DE SEED DE DATOS
            services.AddScoped<ISeedDataService, TestSeedDataService>();
            
            // 🔧 REGISTRAR SERVICIOS MOCK ADICIONALES PARA EVENT HANDLERS
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.ISMSService, MockSMSService>();

            // 🔧 REGISTRAR CACHE FAKE PARA HANDLERS QUE LO REQUIEREN
            services.AddSingleton<RestaurantePro.Domain.Core.SharedKernel.Services.Cache.ICacheService, TestCacheService>();
            
            // 🔧 NO MOCKEAR EL SERVICIO DE ANALYTICS - USAR EL REAL PARA DATOS DE BD
            Console.WriteLine("🔧 Usando AnalyticsService real para obtener datos de la BD");
            
            // 🔧 REGISTRAR TEST NOTIFICATION SERVICE
            Console.WriteLine("🔧 Registrando TestNotificationService en MobileIntegrationTestFixture");
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Notifications.INotificationService, TestNotificationService>();
            
            // 🔧 REGISTRAR TEST COMANDA REALTIME SERVICE
            Console.WriteLine("🔧 Registrando TestComandaRealtimeService en MobileIntegrationTestFixture");
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Realtime.IComandaRealtimeService, TestComandaRealtimeService>();
            
            // 🔧 REGISTRAR HTTPCLIENT PARA SERVICIOS MOBILE
            services.AddHttpClient();
            
            // 🔧 REGISTRAR SERVICIOS MOCK DE MOBILE CORE
            Console.WriteLine("🔧 Registrando servicios mock de Mobile Core en MobileIntegrationTestFixture");
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Platform.ISecureStorageService, TestSecureStorageService>();
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Api.IApiService, TestApiService>();
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Authentication.IAuthService, TestAuthService>();
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Commercial.IClientesService, TestClientesService>();
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Commercial.ITarjetasFidelizacionService, TestTarjetasFidelizacionService>();
            services.AddScoped<RestaurantePro.Mobile.Core.Services.Inventory.IIngredientesService, TestIngredientesService>();
            
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
                // 🔧 LIMPIAR ESTADO COMPARTIDO (cache, etc.) ANTES DE DISPOSE
                try
                {
                    using var scope = Services.CreateScope();
                    var cache = scope.ServiceProvider.GetService<RestaurantePro.Domain.Core.SharedKernel.Services.Cache.ICacheService>() as TestCacheService;
                    cache?.Clear();
                }
                catch { /* best-effort */ }
                // 🔧 LIMPIAR RECURSOS DEL FIXTURE
                base.Dispose(disposing);
            }
            _disposed = true;
        }
    }
}

/// <summary>
/// Configura Identity para tests sin JWT Bearer
/// </summary>
public static class TestIdentityConfiguration
{
    public static void ConfigureIdentityForTests(IServiceCollection services, IConfiguration configuration)
    {
        // Configurar opciones de JWT (solo para compatibilidad, no se usará)
        services.Configure<RestaurantePro.Infrastructure.Identity.Configuration.JwtConfiguration>(configuration.GetSection("JwtSettings"));
        services.AddSingleton<IConfigureOptions<RestaurantePro.Infrastructure.Identity.Configuration.JwtConfiguration>, RestaurantePro.Infrastructure.Identity.Configuration.JwtConfigurationSetup>();

        // Configurar Identity desde el archivo de configuración
        services.Configure<RestaurantePro.Infrastructure.Identity.Configuration.IdentityConfiguration>(configuration.GetSection("IdentitySettings"));
        var identitySettings = configuration.GetSection("IdentitySettings").Get<RestaurantePro.Infrastructure.Identity.Configuration.IdentityConfiguration>() ?? new RestaurantePro.Infrastructure.Identity.Configuration.IdentityConfiguration();

        services.AddIdentity<RestaurantePro.Infrastructure.Identity.Models.ApplicationUser, RestaurantePro.Infrastructure.Identity.Models.ApplicationRole>(options =>
        {
            // Configuración de contraseñas
            options.Password.RequireDigit = identitySettings.PasswordSettings.RequireDigit;
            options.Password.RequireLowercase = identitySettings.PasswordSettings.RequireLowercase;
            options.Password.RequireUppercase = identitySettings.PasswordSettings.RequireUppercase;
            options.Password.RequireNonAlphanumeric = identitySettings.PasswordSettings.RequireNonAlphanumeric;
            options.Password.RequiredLength = identitySettings.PasswordSettings.RequiredLength;
            options.Password.RequiredUniqueChars = identitySettings.PasswordSettings.RequiredUniqueChars;

            // Configuración de bloqueo
            options.Lockout.DefaultLockoutTimeSpan = identitySettings.LockoutSettings.DefaultLockoutTimeSpan;
            options.Lockout.MaxFailedAccessAttempts = identitySettings.LockoutSettings.MaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = identitySettings.LockoutSettings.AllowedForNewUsers;

            // Configuración de usuario
            options.User.RequireUniqueEmail = identitySettings.UserSettings.RequireUniqueEmail;
            
            // Configuración de SignIn
            options.SignIn.RequireConfirmedAccount = identitySettings.UserSettings.RequireConfirmedAccount;
            options.SignIn.RequireConfirmedEmail = identitySettings.UserSettings.RequireConfirmedEmail;
            options.SignIn.RequireConfirmedPhoneNumber = identitySettings.UserSettings.RequireConfirmedPhoneNumber;
        })
        .AddEntityFrameworkStores<RestauranteProDbContext>()
        .AddDefaultTokenProviders()
        .AddRoles<RestaurantePro.Infrastructure.Identity.Models.ApplicationRole>()
        .AddRoleManager<RoleManager<RestaurantePro.Infrastructure.Identity.Models.ApplicationRole>>()
        .AddRoleValidator<RoleValidator<RestaurantePro.Infrastructure.Identity.Models.ApplicationRole>>();

        // Registrar servicios de Identity
        services.AddScoped<RestaurantePro.Application.Common.Interfaces.IJwtTokenService, RestaurantePro.Infrastructure.Identity.Services.JwtTokenService>();
        services.AddScoped<RestaurantePro.Application.Common.Interfaces.IIdentityService, RestaurantePro.Infrastructure.Identity.Services.IdentityService>();
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

public class TestCacheService : RestaurantePro.Domain.Core.SharedKernel.Services.Cache.ICacheService
{
    private readonly Dictionary<string, object?> _store = new();

    public bool Exists(string key) => _store.ContainsKey(key);
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(Exists(key));
    public T Get<T>(string key) => _store.TryGetValue(key, out var v) && v is T t ? t : default!;
    public Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult(Get<T>(key));
    public void InvalidatePattern(string pattern)
    {
        var keys = _store.Keys.Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var k in keys) _store.Remove(k);
    }
    public Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        InvalidatePattern(pattern);
        return Task.CompletedTask;
    }
    public void Remove(string key) => _store.Remove(key);
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) { Remove(key); return Task.CompletedTask; }
    public void Set<T>(string key, T value, int expirationMinutes = 60) => _store[key] = value;
    public Task SetAsync<T>(string key, T value, int expirationMinutes = 60, CancellationToken cancellationToken = default) { Set(key, value, expirationMinutes); return Task.CompletedTask; }
    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) { Set(key, value, (int)expiration.TotalMinutes); return Task.CompletedTask; }
    public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
    {
        if (_store.TryGetValue(key, out var v) && v is T t) return t;
        var created = factory();
        _store[key] = created;
        return created;
    }
    public T GetOrAdd<T>(string key, Func<T> loadFunc, int timeToLiveMinutes = 10) => GetOrCreate(key, loadFunc, timeToLiveMinutes);
    public Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> loadFunc, int timeToLiveMinutes = 10, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var v) && v is T t) return Task.FromResult(t);
        return loadAndSet();
        async Task<T> loadAndSet()
        {
            var created = await loadFunc(cancellationToken);
            _store[key] = created;
            return created;
        }
    }

    public void Clear() => _store.Clear();
}

/// <summary>
/// Implementación de prueba del servicio de notificaciones para tests móviles
/// </summary>
public class TestNotificationService : RestaurantePro.Mobile.Core.Services.Notifications.INotificationService
{
    public List<string> ToastMessages { get; } = new();
    public List<int> VibrationDurations { get; } = new();

    public async Task ShowToastAsync(string message, int durationMs = 2000)
    {
        ToastMessages.Add($"{message} (Duration: {durationMs}ms)");
        await Task.CompletedTask;
    }

    public Task VibrateAsync(int milliseconds = 100)
    {
        VibrationDurations.Add(milliseconds);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Implementación de prueba del servicio de tiempo real para tests móviles
/// </summary>
public class TestComandaRealtimeService : RestaurantePro.Mobile.Core.Services.Realtime.IComandaRealtimeService
{
    public event Action? OnNuevaComanda;
    public event Action? OnComandaActualizada;

    public bool IsStarted { get; private set; }
    public bool IsStopped { get; private set; }
    public int StartCallCount { get; private set; }
    public int StopCallCount { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        StartCallCount++;
        IsStarted = true;
        IsStopped = false;
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        StopCallCount++;
        IsStopped = true;
        IsStarted = false;
        await Task.CompletedTask;
    }

    // Métodos para simular eventos en tests
    public void SimulateNuevaComanda()
    {
        OnNuevaComanda?.Invoke();
    }

    public void SimulateComandaActualizada()
    {
        OnComandaActualizada?.Invoke();
    }
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


/// <summary>
/// Implementación de prueba del servicio de almacenamiento seguro para tests móviles
/// </summary>
public class TestSecureStorageService : RestaurantePro.Mobile.Core.Services.Platform.ISecureStorageService
{
    private readonly Dictionary<string, string> _store = new();

    public Task<string?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(string key, string value)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _store.Clear();
        return Task.CompletedTask;
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
        Console.WriteLine($"🔍 TestAuthenticationHandler: Iniciando autenticación...");
        Console.WriteLine($"🔍 TestAuthenticationHandler: URL: {Request.Path}");
        Console.WriteLine($"🔍 TestAuthenticationHandler: Método: {Request.Method}");
        Console.WriteLine($"🔍 TestAuthenticationHandler: Headers disponibles: {string.Join(", ", Request.Headers.Keys)}");
        
        // Verificar si hay un header de autorización (X-Bearer-Token o Authorization)
        string? authHeader = null;
        if (Request.Headers.ContainsKey("X-Bearer-Token"))
        {
            authHeader = Request.Headers["X-Bearer-Token"].ToString();
            Console.WriteLine($"🔍 TestAuthenticationHandler: Header X-Bearer-Token encontrado: {authHeader}");
        }
        else if (Request.Headers.ContainsKey("Authorization"))
        {
            authHeader = Request.Headers["Authorization"].ToString();
            Console.WriteLine($"🔍 TestAuthenticationHandler: Header Authorization encontrado: {authHeader}");
        }
        else
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: NO hay headers de autorización");
            return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
        }
        Console.WriteLine($"🔍 TestAuthenticationHandler: Header recibido: {authHeader}");
        
        // Si es un token JWT válido (Bearer), procesarlo y devolver autenticación exitosa
        if (authHeader.StartsWith("Bearer "))
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: Procesando Bearer token");
            return HandleBearerSchemeAsync(authHeader);
        }
        
        // Si es un token JWT directo (sin prefijo Bearer), procesarlo
        if (authHeader.Contains(".") && authHeader.Split('.').Length == 3)
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: Procesando JWT token directo");
            return HandleBearerSchemeAsync($"Bearer {authHeader}");
        }
        
        // Soporte para esquema "Test" (comportamiento original)
        if (authHeader.StartsWith("Test "))
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: Procesando Test scheme");
            var result = HandleTestSchemeAsync(authHeader);
            Console.WriteLine($"🔍 TestAuthenticationHandler: Resultado de Test scheme: {result.Result.Succeeded}");
            return result;
        }
        
        Console.WriteLine($"🔍 TestAuthenticationHandler: Esquema no reconocido");
        return Task.FromResult(AuthenticateResult.Fail("Invalid authentication scheme"));
    }

    private Task<AuthenticateResult> HandleTestSchemeAsync(string authHeader)
    {
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
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Role, role),
            new Claim("permission", "perfil.read"),
            new Claim("permission", "perfil.update")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private Task<AuthenticateResult> HandleBearerSchemeAsync(string authHeader)
    {
        try
        {
            // Extraer el token JWT
            var token = authHeader.Substring(7); // "Bearer " tiene 7 caracteres
            
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Token recibido: {token}");
            
            // 🔧 PARSER BÁSICO DE JWT PARA TESTS
            // En tests, vamos a extraer información básica del token JWT sin validación criptográfica
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Token no es JWT válido (partes: {parts.Length}), usando fallback");
                // Si no es un JWT válido, aceptar cualquier token fake para tests
                var fallbackClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "fake-user-id"),
                    new Claim(ClaimTypes.Name, "FakeUser"),
                    new Claim(ClaimTypes.Email, "fake@test.com"),
                    new Claim(ClaimTypes.Role, "Empleado"),
                    new Claim("permission", "perfil.read"),
                    new Claim("permission", "perfil.update")
                };
                
                var fallbackIdentity = new ClaimsIdentity(fallbackClaims, "Test");
                var fallbackPrincipal = new ClaimsPrincipal(fallbackIdentity);
                
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(fallbackPrincipal, "Test")));
            }
            
            // Intentar parsear el payload del JWT
            var payload = parts[1];
            // Agregar padding si es necesario
            var padding = 4 - (payload.Length % 4);
            if (padding != 4)
            {
                payload += new string('=', padding);
            }
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Payload: {payloadJson}");
            
            // Parsear el JSON del payload
            var payloadObj = JsonSerializer.Deserialize<JsonObject>(payloadJson);
            if (payloadObj == null)
            {
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - No se pudo parsear el payload JSON");
                return Task.FromResult(AuthenticateResult.Fail("Invalid JWT payload"));
            }
            
            // Extraer claims del JWT
            var claims = new List<Claim>();
            
            // NameIdentifier (sub) - Preservar tanto el claim "sub" como NameIdentifier
            if (payloadObj.TryGetPropertyValue("sub", out var subValue))
            {
                var subString = subValue?.ToString() ?? "unknown";
                claims.Add(new Claim(ClaimTypes.NameIdentifier, subString));
                claims.Add(new Claim("sub", subString)); // Preservar el claim "sub" original
                claims.Add(new Claim("userid", subString)); // También agregar "userid" como fallback
            }
            
            // Name (name)
            if (payloadObj.TryGetPropertyValue("name", out var nameValue))
            {
                claims.Add(new Claim(ClaimTypes.Name, nameValue?.ToString() ?? "Unknown"));
            }
            
            // Email
            if (payloadObj.TryGetPropertyValue("email", out var emailValue))
            {
                claims.Add(new Claim(ClaimTypes.Email, emailValue?.ToString() ?? "unknown@test.com"));
            }
            
            // UID (identificador adicional)
            if (payloadObj.TryGetPropertyValue("uid", out var uidValue))
            {
                claims.Add(new Claim("uid", uidValue?.ToString() ?? "unknown"));
            }
            
            // Roles - El JWT real puede tener múltiples roles como claims separados
            // Buscar todos los claims de tipo "role" en el payload
            var roleClaims = payloadObj.Where(kvp => kvp.Key == "role").ToList();
            if (roleClaims.Any())
            {
                foreach (var roleClaim in roleClaims)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value?.ToString() ?? "Empleado"));
                }
            }
            else
            {
                // Fallback: buscar en claims estándar de JWT
                if (payloadObj.TryGetPropertyValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var standardRoleValue))
                {
                    claims.Add(new Claim(ClaimTypes.Role, standardRoleValue?.ToString() ?? "Empleado"));
                }
            }
            
            // Agregar claims por defecto si no hay suficientes
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, "admin@restaurantepro.com"));
            }
            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, "admin@restaurantepro.com"));
            }
            if (!claims.Any(c => c.Type == ClaimTypes.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, "admin@restaurantepro.com"));
            }
            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrador"));
            }
            
            // Agregar permisos
            claims.Add(new Claim("permission", "perfil.read"));
            claims.Add(new Claim("permission", "perfil.update"));
            
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Autenticación exitosa con {claims.Count} claims");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Error procesando JWT: {ex.Message}");
            return Task.FromResult(AuthenticateResult.Fail($"Error processing JWT: {ex.Message}"));
        }
    }
}

/// <summary>
/// Implementación de prueba del servicio de autenticación para tests móviles
/// </summary>
public class TestAuthService : RestaurantePro.Mobile.Core.Services.Authentication.IAuthService
{
    public Task<string?> GetTokenAsync()
    {
        // Devolver un token simple para el esquema "Test"
        return Task.FromResult<string?>("Test AuthenticatedUser-Admin");
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        return Task.FromResult(true);
    }

    public Task LogoutAsync()
    {
        return Task.CompletedTask;
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.AuthResponse>> LoginAsync(string email, string password, bool recordarme = false)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.AuthResponse>
        {
            Success = true,
            Data = new RestaurantePro.Mobile.Core.Models.DTOs.AuthResponse
            {
                Token = "Test AuthenticatedUser-Admin",
                RefreshToken = "mock-refresh-token",
                User = new RestaurantePro.Mobile.Core.Models.DTOs.AuthUser
                {
                    Id = 1,
                    Email = email,
                    Nombre = "Test",
                    Apellido = "User",
                    Roles = new List<string> { "Admin" }
                },
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            },
            Message = "Login successful"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.AuthUser?> GetCurrentUserAsync()
    {
        var user = new RestaurantePro.Mobile.Core.Models.DTOs.AuthUser
        {
            Id = 1,
            Email = "admin@restaurantepro.com",
            Nombre = "Admin",
            Apellido = "User",
            Roles = new List<string> { "Admin" }
        };
        return Task.FromResult<RestaurantePro.Mobile.Core.Models.DTOs.AuthUser?>(user);
    }

    public Task<string?> GetUserIdAsync()
    {
        return Task.FromResult<string?>("admin@restaurantepro.com");
    }
}

/// <summary>
/// Implementación de prueba del servicio de API para tests móviles
/// </summary>
public class TestApiService : RestaurantePro.Mobile.Core.Services.Api.IApiService
{
    private readonly HttpClient _httpClient;

    public TestApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthHeader(token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Log para debugging
            Console.WriteLine($"🔍 TestApiService - Endpoint: {endpoint}");
            Console.WriteLine($"🔍 TestApiService - StatusCode: {response.StatusCode}");
            Console.WriteLine($"🔍 TestApiService - Content: {content.Substring(0, Math.Min(200, content.Length))}...");

            if (response.IsSuccessStatusCode)
            {
                // Deserializar la respuesta como ApiResponse<T>
                var apiResponse = JsonSerializer.Deserialize<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (apiResponse != null)
                {
                    return apiResponse;
                }
                
                // Fallback: intentar deserializar directamente como T
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.SuccessResponse(result, "Success");
            }

            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {content}" },
                "Error de conexión",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
        }
    }

    public async Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthHeader(token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            StringContent? content = null;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.SuccessResponse(result, "Success");
            }

            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {responseContent}" },
                "Error de conexión",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
        }
    }

    public async Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthHeader(token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            StringContent? content = null;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.SuccessResponse(result, "Success");
            }

            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {responseContent}" },
                "Error de conexión",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
        }
    }

    public async Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthHeader(token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(endpoint, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.SuccessResponse(result, "Success");
            }

            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {responseContent}" },
                "Error de conexión",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
        }
    }

    public async Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            AddAuthHeader(token);
            if (!_httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>.SuccessResponse(true, "Success");
            }

            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>.ErrorResponse(
                new List<string> { $"Error HTTP: {response.StatusCode} - {responseContent}" },
                "Error de conexión",
                (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>.ErrorResponse(new List<string> { ex.Message }, "Error inesperado", 500);
        }
    }

    private void AddAuthHeader(string? token)
    {
        // Remover headers de autenticación existentes
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Remove("X-Bearer-Token");
        
        if (!string.IsNullOrEmpty(token))
        {
            // Usar el header Authorization para el esquema "Test"
            _httpClient.DefaultRequestHeaders.Add("Authorization", token);
        }
    }
}

/// <summary>
/// Implementación de prueba del servicio de clientes para tests móviles
/// </summary>
public class TestClientesService : RestaurantePro.Mobile.Core.Services.Commercial.IClientesService
{
    private readonly List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto> _clientes = new()
    {
        new RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Ana García",
            Email = "ana@example.com",
            Telefono = "+1234567890",
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-30)
        },
        new RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto
        {
            Id = Guid.NewGuid(),
            NombreCompleto = "Carlos López",
            Email = "carlos@example.com",
            Telefono = "+0987654321",
            Activo = true,
            FechaRegistro = DateTime.UtcNow.AddDays(-15)
        }
    };

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        // Verificar cancelación
        if (cancellationToken.IsCancellationRequested)
        {
            var cancelResponse = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
            {
                Success = false,
                Data = null,
                Message = "Operación cancelada"
            };
            return Task.FromResult(cancelResponse);
        }

        var clientes = soloActivos ? _clientes.Where(c => c.Activo).ToList() : _clientes;
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = clientes,
            Message = "Clientes obtenidos exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>> ObtenerClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);
        if (cliente == null)
        {
            var errorResponse = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
            {
                Success = false,
                Data = null,
                Message = "Cliente no encontrado"
            };
            return Task.FromResult(errorResponse);
        }

        var clienteDto = new RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto
        {
            Id = cliente.Id,
            NombreCompleto = cliente.NombreCompleto,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FechaRegistro = cliente.FechaRegistro,
            Estado = "Activo",
            Activo = cliente.Activo,
            TotalComandas = 5,
            TotalGastado = 150.50m
        };

        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
        {
            Success = true,
            Data = clienteDto,
            Message = "Cliente obtenido exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> BuscarClientesAsync(string terminoBusqueda, CancellationToken cancellationToken = default)
    {
        var clientes = _clientes.Where(c => c.NombreCompleto.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)).ToList();
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = clientes,
            Message = "Clientes encontrados"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>> CrearClienteAsync(RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto cliente, CancellationToken cancellationToken = default)
    {
        if (cliente == null)
        {
            var errorResponse = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
            {
                Success = false,
                Data = null,
                Message = "Cliente no puede ser nulo"
            };
            return Task.FromResult(errorResponse);
        }

        // Verificar cancelación
        if (cancellationToken.IsCancellationRequested)
        {
            var cancelResponse = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
            {
                Success = false,
                Data = null,
                Message = "Operación cancelada"
            };
            return Task.FromResult(cancelResponse);
        }

        var nuevoCliente = new RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto
        {
            Id = Guid.NewGuid(),
            NombreCompleto = cliente.NombreCompleto,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            FechaRegistro = DateTime.UtcNow,
            Estado = "Activo",
            Activo = true,
            TotalComandas = 0,
            TotalGastado = 0
        };

        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
        {
            Success = true,
            Data = nuevoCliente,
            Message = "Cliente creado exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>> ActualizarClienteAsync(Guid id, RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto cliente, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.ClienteDto>
        {
            Success = true,
            Data = cliente,
            Message = "Cliente actualizado exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> EliminarClienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Cliente eliminado exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasClientesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        var estadisticas = new RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasClientesDto
        {
            TotalClientes = _clientes.Count,
            ClientesActivos = _clientes.Count(c => c.Activo),
            ClientesNuevosHoy = 0,
            ClientesInactivos = 0
        };

        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasClientesDto>
        {
            Success = true,
            Data = estadisticas,
            Message = "Estadísticas obtenidas exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesFrecuentesAsync(int cantidad = 10, CancellationToken cancellationToken = default)
    {
        var clientes = _clientes.Take(cantidad).ToList();
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = clientes,
            Message = "Clientes frecuentes obtenidos"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesAsync(RestaurantePro.Mobile.Core.Models.DTOs.FiltroClientesDto filtro, CancellationToken cancellationToken = default)
    {
        var clientes = _clientes.AsQueryable();

        if (filtro.SoloActivos.HasValue && filtro.SoloActivos.Value)
            clientes = clientes.Where(c => c.Activo);

        if (!string.IsNullOrEmpty(filtro.Busqueda))
            clientes = clientes.Where(c => c.NombreCompleto.Contains(filtro.Busqueda, StringComparison.OrdinalIgnoreCase));

        var resultado = clientes.ToList();
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = resultado,
            Message = "Clientes obtenidos con filtro"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesPorSegmentoAsync(string segmento, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = _clientes,
            Message = $"Clientes del segmento {segmento} obtenidos"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesConTarjetaFidelizacionAsync(CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = _clientes,
            Message = "Clientes con tarjeta de fidelización obtenidos"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>> ObtenerClientesPorFechaRegistroAsync(DateTime fechaDesde, DateTime fechaHasta, CancellationToken cancellationToken = default)
    {
        var clientes = _clientes.Where(c => c.FechaRegistro >= fechaDesde && c.FechaRegistro <= fechaHasta).ToList();
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ClienteSummaryDto>>
        {
            Success = true,
            Data = clientes,
            Message = "Clientes por fecha de registro obtenidos"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> DesactivarClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Cliente desactivado exitosamente"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ComandaDto>>> ObtenerHistorialComandasAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        var comandas = new List<RestaurantePro.Mobile.Core.Models.DTOs.ComandaDto>();
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ComandaDto>>
        {
            Success = true,
            Data = comandas,
            Message = "Historial de comandas obtenido"
        };
        return Task.FromResult(response);
    }
}

/// <summary>
/// Mock del servicio de tarjetas de fidelización para tests
/// </summary>
public class TestTarjetasFidelizacionService : RestaurantePro.Mobile.Core.Services.Commercial.ITarjetasFidelizacionService
{
    private readonly List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto> _tarjetas = new()
    {
        new RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            CodigoTarjeta = "TARJETA001",
            NumeroTarjeta = "TARJETA001",
            PuntosDisponibles = 1000,
            Estado = "Activa",
            Activa = true,
            FechaActivacion = DateTime.Now.AddDays(-30),
            ClienteId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            ClienteNombre = "Cliente Test 1"
        },
        new RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            CodigoTarjeta = "TARJETA002",
            NumeroTarjeta = "TARJETA002",
            PuntosDisponibles = 500,
            Estado = "Activa",
            Activa = true,
            FechaActivacion = DateTime.Now.AddDays(-15),
            ClienteId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
            ClienteNombre = "Cliente Test 2"
        }
    };

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> BuscarTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.NumeroTarjeta == numeroTarjeta);
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Tarjeta encontrada" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> ActivarTarjetaAsync(string numeroTarjeta, string nombreCliente, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.NumeroTarjeta == numeroTarjeta);
        if (tarjeta != null)
        {
            tarjeta.Estado = "Activa";
            tarjeta.Activa = true;
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Tarjeta activada exitosamente" : "Tarjeta no encontrada para activar"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> ObtenerTarjetaPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.CodigoTarjeta == codigo || t.NumeroTarjeta == codigo);
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Tarjeta encontrada" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> ObtenerTarjetaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == id);
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Tarjeta obtenida" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>>> ObtenerHistorialTransaccionesAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta == null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>>
            {
                Success = false,
                Message = "Tarjeta no encontrada"
            };
            return Task.FromResult(response);
        }

        var transacciones = new List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>
        {
            new RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto
            {
                Id = Guid.NewGuid(),
                TipoTransaccion = "Acumulación",
                Puntos = 100,
                Monto = 50.00m,
                FechaTransaccion = DateTime.Now.AddDays(-1),
                Descripcion = "Compra en restaurante"
            }
        };

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>>
        {
            Success = true,
            Data = transacciones,
            Message = "Historial de transacciones obtenido"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> AcumularPuntosAsync(Guid tarjetaId, decimal montoCompra, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        if (montoCompra <= 0)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "El monto debe ser mayor a cero"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta != null)
        {
            var puntosGanados = (int)(montoCompra * 2); // 2 puntos por cada peso
            tarjeta.PuntosDisponibles += puntosGanados;
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Puntos acumulados exitosamente" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>> CanjearPuntosAsync(Guid tarjetaId, int puntosACanjear, decimal descuento, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        if (puntosACanjear <= 0)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Los puntos a canjear deben ser mayores a cero"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta != null && tarjeta.PuntosDisponibles >= puntosACanjear)
        {
            tarjeta.PuntosDisponibles -= puntosACanjear;
        }
        else if (tarjeta != null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
            {
                Success = false,
                Message = "Puntos insuficientes para el canje"
            };
            return Task.FromResult(response);
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>
        {
            Success = tarjeta != null,
            Data = tarjeta,
            Message = tarjeta != null ? "Puntos canjeados exitosamente" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>> ObtenerTarjetasActivasAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjetasActivas = _tarjetas.Where(t => t.Estado == "Activa").ToList();
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>
        {
            Success = true,
            Data = tarjetasActivas,
            Message = "Tarjetas activas obtenidas"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> DesactivarTarjetaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == id);
        if (tarjeta != null)
        {
            tarjeta.Estado = "Inactiva";
            tarjeta.Activa = false;
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = tarjeta != null,
            Data = tarjeta != null,
            Message = tarjeta != null ? "Tarjeta desactivada exitosamente" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>> ObtenerTarjetasAsync(RestaurantePro.Mobile.Core.Models.DTOs.FiltroTarjetasFidelizacionDto filtro, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjetas = _tarjetas.AsQueryable();

        if (!string.IsNullOrEmpty(filtro.Estado))
            tarjetas = tarjetas.Where(t => t.Estado == filtro.Estado);

        if (filtro.SoloActivas.HasValue && filtro.SoloActivas.Value)
            tarjetas = tarjetas.Where(t => t.Activa);

        var resultado = tarjetas.ToList();
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TarjetaFidelizacionDto>>
        {
            Success = true,
            Data = resultado,
            Message = "Tarjetas obtenidas con filtro"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>>> ObtenerHistorialAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        return ObtenerHistorialTransaccionesAsync(tarjetaId, cancellationToken);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> BloquearTarjetaAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta != null)
        {
            tarjeta.Estado = "Bloqueada";
            tarjeta.Activa = false;
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = tarjeta != null,
            Data = tarjeta != null,
            Message = tarjeta != null ? "Tarjeta bloqueada exitosamente" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.HistorialPuntosDto>> ObtenerHistorialPuntosAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.HistorialPuntosDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta == null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.HistorialPuntosDto>
            {
                Success = false,
                Message = "Tarjeta no encontrada"
            };
            return Task.FromResult(response);
        }

        var historial = new RestaurantePro.Mobile.Core.Models.DTOs.HistorialPuntosDto
        {
            Id = Guid.NewGuid(),
            CodigoTarjeta = tarjeta.CodigoTarjeta,
            PuntosAcumulados = 1000,
            PuntosCanjeados = 200,
            PuntosDisponibles = tarjeta.PuntosDisponibles,
            FechaUltimaTransaccion = DateTime.Now.AddDays(-1),
            Transacciones = new List<RestaurantePro.Mobile.Core.Models.DTOs.TransaccionPuntosDto>()
        };

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.HistorialPuntosDto>
        {
            Success = true,
            Data = historial,
            Message = "Historial de puntos obtenido"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasTarjetaDto>> ObtenerEstadisticasAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasTarjetaDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta == null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasTarjetaDto>
            {
                Success = false,
                Message = "Tarjeta no encontrada"
            };
            return Task.FromResult(response);
        }

        var estadisticas = new RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasTarjetaDto
        {
            TarjetaId = tarjetaId,
            CodigoTarjeta = tarjeta.CodigoTarjeta,
            PuntosAcumulados = 1000,
            PuntosCanjeados = 200,
            PuntosDisponibles = tarjeta.PuntosDisponibles,
            MontoTotalGastado = 5000.00m,
            TotalTransacciones = 25,
            FechaUltimaTransaccion = DateTime.Now.AddDays(-1),
            NivelActual = "Oro",
            PuntosParaSiguienteNivel = 500,
            BeneficiosDisponibles = "Descuentos especiales, productos exclusivos"
        };

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasTarjetaDto>
        {
            Success = true,
            Data = estadisticas,
            Message = "Estadísticas obtenidas"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> EliminarTarjetaAsync(Guid tarjetaId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var tarjeta = _tarjetas.FirstOrDefault(t => t.Id == tarjetaId);
        if (tarjeta != null)
        {
            _tarjetas.Remove(tarjeta);
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = tarjeta != null,
            Data = tarjeta != null,
            Message = tarjeta != null ? "Tarjeta eliminada exitosamente" : "Tarjeta no encontrada"
        };
        return Task.FromResult(response2);
    }
}

/// <summary>
/// Mock del servicio de ingredientes para tests
/// </summary>
public class TestIngredientesService : RestaurantePro.Mobile.Core.Services.Inventory.IIngredientesService
{
    private readonly List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto> _ingredientes = new()
    {
        new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Nombre = "Tomate",
            Descripcion = "Tomate fresco para ensaladas",
            StockActual = 50,
            StockMinimo = 10,
            UnidadMedida = "kg",
            PrecioUnitario = 2.50m,
            Categoria = "Vegetales",
            Disponible = true,
            FechaCreacion = DateTime.Now.AddDays(-30),
            Proveedor = "Proveedor A"
        },
        new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Nombre = "Lechuga",
            Descripcion = "Lechuga fresca",
            StockActual = 5,
            StockMinimo = 15,
            UnidadMedida = "unidades",
            PrecioUnitario = 1.20m,
            Categoria = "Vegetales",
            Disponible = true,
            FechaCreacion = DateTime.Now.AddDays(-15),
            Proveedor = "Proveedor B"
        },
        new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Nombre = "Carne de Res",
            Descripcion = "Carne de res premium",
            StockActual = 0,
            StockMinimo = 5,
            UnidadMedida = "kg",
            PrecioUnitario = 25.00m,
            Categoria = "Carnes",
            Disponible = false,
            FechaCreacion = DateTime.Now.AddDays(-10),
            Proveedor = "Proveedor C"
        }
    };

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>> ObtenerIngredientesAsync(bool soloActivos = true, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingredientes = _ingredientes.AsQueryable();
        if (soloActivos)
        {
            ingredientes = ingredientes.Where(i => i.Disponible);
        }

        var summaryDtos = ingredientes.Select(i => new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto
        {
            Id = i.Id,
            Nombre = i.Nombre,
            StockActual = i.StockActual,
            StockMinimo = i.StockMinimo,
            UnidadMedida = i.UnidadMedida,
            Disponible = i.Disponible
        }).ToList();

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
        {
            Success = true,
            Data = summaryDtos,
            Message = "Ingredientes obtenidos exitosamente"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>> BuscarIngredientesAsync(string terminoBusqueda, string? categoria = null, bool? soloDisponibles = null, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingredientes = _ingredientes.AsQueryable();

        if (!string.IsNullOrEmpty(terminoBusqueda))
        {
            ingredientes = ingredientes.Where(i => i.Nombre.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(categoria))
        {
            ingredientes = ingredientes.Where(i => i.Categoria == categoria);
        }

        if (soloDisponibles.HasValue)
        {
            ingredientes = ingredientes.Where(i => i.Disponible == soloDisponibles.Value);
        }

        var summaryDtos = ingredientes.Select(i => new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto
        {
            Id = i.Id,
            Nombre = i.Nombre,
            StockActual = i.StockActual,
            StockMinimo = i.StockMinimo,
            UnidadMedida = i.UnidadMedida,
            Disponible = i.Disponible
        }).ToList();

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
        {
            Success = true,
            Data = summaryDtos,
            Message = "Búsqueda de ingredientes completada"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>> ObtenerIngredientesBajoStockAsync(int stockMinimo = 10, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingredientes = _ingredientes.Where(i => i.StockActual <= stockMinimo).Select(i => new RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto
        {
            Id = i.Id,
            Nombre = i.Nombre,
            StockActual = i.StockActual,
            StockMinimo = i.StockMinimo,
            UnidadMedida = i.UnidadMedida,
            Disponible = i.Disponible
        }).ToList();

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteSummaryDto>>
        {
            Success = true,
            Data = ingredientes,
            Message = "Ingredientes bajo stock obtenidos"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>> ObtenerIngredienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingrediente = _ingredientes.FirstOrDefault(i => i.Id == id);
        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
        {
            Success = ingrediente != null,
            Data = ingrediente,
            Message = ingrediente != null ? "Ingrediente obtenido" : "Ingrediente no encontrado"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>> CrearIngredienteAsync(RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto ingrediente, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        if (ingrediente == null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
            {
                Success = false,
                Message = "Ingrediente no puede ser nulo"
            };
            return Task.FromResult(response);
        }

        ingrediente.Id = Guid.NewGuid();
        ingrediente.FechaCreacion = DateTime.Now;
        _ingredientes.Add(ingrediente);

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
        {
            Success = true,
            Data = ingrediente,
            Message = "Ingrediente creado exitosamente"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>> ActualizarIngredienteAsync(Guid id, RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto ingrediente, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingredienteExistente = _ingredientes.FirstOrDefault(i => i.Id == id);
        if (ingredienteExistente != null && ingrediente != null)
        {
            ingredienteExistente.Nombre = ingrediente.Nombre;
            ingredienteExistente.Descripcion = ingrediente.Descripcion;
            ingredienteExistente.StockActual = ingrediente.StockActual;
            ingredienteExistente.StockMinimo = ingrediente.StockMinimo;
            ingredienteExistente.UnidadMedida = ingrediente.UnidadMedida;
            ingredienteExistente.PrecioUnitario = ingrediente.PrecioUnitario;
            ingredienteExistente.Categoria = ingrediente.Categoria;
            ingredienteExistente.Disponible = ingrediente.Disponible;
            ingredienteExistente.Proveedor = ingrediente.Proveedor;
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.IngredienteDto>
        {
            Success = ingredienteExistente != null,
            Data = ingredienteExistente,
            Message = ingredienteExistente != null ? "Ingrediente actualizado exitosamente" : "Ingrediente no encontrado"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> EliminarIngredienteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingrediente = _ingredientes.FirstOrDefault(i => i.Id == id);
        if (ingrediente != null)
        {
            _ingredientes.Remove(ingrediente);
        }

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = ingrediente != null,
            Data = ingrediente != null,
            Message = ingrediente != null ? "Ingrediente eliminado exitosamente" : "Ingrediente no encontrado"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasIngredientesDto>> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasIngredientesDto>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var estadisticas = new RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasIngredientesDto
        {
            TotalIngredientes = _ingredientes.Count,
            IngredientesDisponibles = _ingredientes.Count(i => i.Disponible),
            IngredientesBajoStock = _ingredientes.Count(i => i.StockActual <= i.StockMinimo),
            IngredientesAgotados = _ingredientes.Count(i => i.StockActual == 0),
            ValorTotalInventario = _ingredientes.Sum(i => i.StockActual * i.PrecioUnitario),
            TopIngredientesBajoStock = _ingredientes.Where(i => i.StockActual <= i.StockMinimo).Take(5).ToList()
        };

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<RestaurantePro.Mobile.Core.Models.DTOs.EstadisticasIngredientesDto>
        {
            Success = true,
            Data = estadisticas,
            Message = "Estadísticas obtenidas exitosamente"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ReporteValoracionDto>>> GenerarReporteValoracionAsync(RestaurantePro.Mobile.Core.Models.DTOs.FiltroIngredientesDto filtro, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ReporteValoracionDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var reportes = _ingredientes.Select(i => new RestaurantePro.Mobile.Core.Models.DTOs.ReporteValoracionDto
        {
            Id = i.Id,
            NombreIngrediente = i.Nombre,
            ValorActual = i.StockActual * i.PrecioUnitario,
            ValorPromedio = i.StockActual * i.PrecioUnitario * 0.9m,
            VariacionPorcentual = 10.0m,
            FechaReporte = DateTime.Now,
            Tendencia = "Estable"
        }).ToList();

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.ReporteValoracionDto>>
        {
            Success = true,
            Data = reportes,
            Message = "Reporte de valoración generado exitosamente"
        };
        return Task.FromResult(response2);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto>>> ObtenerMovimientosAsync(Guid ingredienteId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto>>
            {
                Success = false,
                Message = "Operación cancelada"
            };
            return Task.FromResult(response);
        }

        var ingrediente = _ingredientes.FirstOrDefault(i => i.Id == ingredienteId);
        if (ingrediente == null)
        {
            var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto>>
            {
                Success = false,
                Message = "Ingrediente no encontrado"
            };
            return Task.FromResult(response);
        }

        var movimientos = new List<RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto>
        {
            new RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto
            {
                Id = Guid.NewGuid(),
                IngredienteId = ingredienteId,
                TipoMovimiento = "Entrada",
                Cantidad = 100,
                Observaciones = "Compra inicial",
                FechaMovimiento = DateTime.Now.AddDays(-30),
                UsuarioResponsable = "Admin",
                StockAnterior = 0,
                StockPosterior = 100
            },
            new RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto
            {
                Id = Guid.NewGuid(),
                IngredienteId = ingredienteId,
                TipoMovimiento = "Salida",
                Cantidad = 50,
                Observaciones = "Consumo en preparaciones",
                FechaMovimiento = DateTime.Now.AddDays(-15),
                UsuarioResponsable = "Cocinero",
                StockAnterior = 100,
                StockPosterior = 50
            }
        };

        var response2 = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<List<RestaurantePro.Mobile.Core.Models.DTOs.MovimientoInventarioDto>>
        {
            Success = true,
            Data = movimientos,
            Message = "Movimientos obtenidos exitosamente"
        };
        return Task.FromResult(response2);
    }
} 
