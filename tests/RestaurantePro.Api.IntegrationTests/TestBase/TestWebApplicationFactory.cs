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
using Microsoft.EntityFrameworkCore.Diagnostics;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Infrastructure.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace RestaurantePro.Api.IntegrationTests.TestBase;

/// <summary>
/// Factory personalizada para configurar la aplicación web en los tests de integración.
/// Configura base de datos SQLite temporal única por test para evitar conflictos de concurrencia.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // 🔧 CONFIGURACIÓN DE SQLITE TEMPORAL ÚNICA POR TEST
    private readonly string _databasePath;
    private readonly string _connectionString;
    private bool _disposed = false;

    public TestWebApplicationFactory()
    {
        // Crear archivo temporal único para cada instancia de test
        _databasePath = Path.GetTempFileName();
        _connectionString = $"Data Source={_databasePath};Cache=Private";
        
        Console.WriteLine($"🗄️ Creando BD temporal única: {_databasePath}");
    }

    public string DatabasePath => _databasePath;
    public string ConnectionString => _connectionString;
    
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
            // Configuración específica para testing con base de datos temporal
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", _connectionString},
                {"Logging:LogLevel:Default", "Information"}, // Habilitar logs para debugging
                {"Logging:LogLevel:Microsoft", "Warning"},
                {"Logging:LogLevel:Microsoft.Hosting.Lifetime", "Warning"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command", "Warning"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Connection", "Warning"},
                {"Logging:LogLevel:Microsoft.EntityFrameworkCore.Update", "Warning"},
                {"Logging:LogLevel:RestaurantePro.Application.Common.Behaviors", "Information"},
                {"Logging:LogLevel:RestaurantePro.Infrastructure.Persistence", "Information"},
                {"TESTING_MODE", "true"}
            });
        });

        // 🔧 CONFIGURACIÓN SIMPLIFICADA PARA TESTS
        builder.ConfigureServices((context, services) =>
        {
            // Obtener la configuración del contexto
            var configuration = context.Configuration;
            
            // 🔧 CONFIGURACIÓN DE BASE DE DATOS TEMPORAL ÚNICA POR TEST
            Console.WriteLine($"🗄️ Configurando BD temporal: {_databasePath}");
            
            // 🔧 CONFIGURAR DBCONTEXT COMO SCOPED PARA TESTS (MEJOR PRÁCTICA)
            services.AddDbContext<RestauranteProDbContext>((provider, options) =>
            {
                options.UseSqlite(_connectionString, sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly("RestaurantePro.Infrastructure");
                });
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.ConfigureWarnings(warnings => warnings
                    .Ignore(RelationalEventId.PendingModelChangesWarning)
                    .Ignore(RelationalEventId.MultipleCollectionIncludeWarning)
                    .Ignore(CoreEventId.RowLimitingOperationWithoutOrderByWarning));
            });

            // 🔧 CONFIGURAR PROVEEDORESDBCONTEXT COMO SCOPED PARA TESTS
            services.AddDbContext<ProveedoresDbContext>((provider, options) =>
            {
                options.UseSqlite(_connectionString, sqliteOptions =>
                {
                    sqliteOptions.MigrationsAssembly("RestaurantePro.Infrastructure");
                });
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // 🔧 REGISTRAR IDOMAINEVENTDISPATCHER COMO SCOPED PARA TESTS
            services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();
            
            // 🔧 AGREGAR INTERCEPTORES PARA EVENTOS DE DOMINIO
            services.AddScoped<RestaurantePro.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor>();
            services.AddScoped<RestaurantePro.Infrastructure.Persistence.Interceptors.DomainEventInterceptor>();
            services.AddScoped<RestaurantePro.Infrastructure.Persistence.Interceptors.SoftDeleteInterceptor>();

            // 🔧 REGISTRAR IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // 🔧 REGISTRAR IProveedoresDbContext
            services.AddScoped<IProveedoresDbContext>(provider => 
                provider.GetRequiredService<ProveedoresDbContext>());

            // 🔧 REGISTRAR DbContext GENÉRICO PARA REPOSITORIOS
            services.AddScoped<DbContext>(provider => 
                provider.GetRequiredService<RestauranteProDbContext>());

            // REGISTRO GLOBAL DE INFRAESTRUCTURA PARA TESTS
            services.AddInfrastructureServices(configuration, isTestEnvironment: true);

            // 🔧 CONFIGURAR SEED DATA PARA TESTS
            services.Configure<RestaurantePro.Infrastructure.Persistence.SeedData.Extensions.SeedDataConfiguration>(options =>
            {
                options.RunCriticalData = true;  // Siempre ejecutar datos críticos en tests
                options.RunDemoData = false;     // No ejecutar datos demo en tests
                options.RunTestingData = true;   // Ejecutar datos de testing
                options.CreateAdminUser = false; // No crear usuario admin en tests
                options.ForceReseed = false;     // No forzar re-seed
            });

            // 🔧 EJECUTAR MIGRACIONES PARA CREAR TABLAS EN BD TEMPORAL
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
                try
                {
                    // Forzar eliminación completa de BD para asegurar esquema limpio
                    dbContext.Database.EnsureDeleted();
                    Console.WriteLine("🗑️ Base de datos eliminada completamente");
                    
                    // Usar EnsureCreated para tests con SQLite (evita problemas de sintaxis de migraciones)
                    dbContext.Database.EnsureCreated();
                    Console.WriteLine("✅ Tablas creadas en BD temporal usando EnsureCreated");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error ejecutando migraciones: {ex.Message}");
                    throw;
                }
            }

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
                    .AddAuthenticationSchemes("Test", "Bearer")
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
        if (disposing && !_disposed)
        {
            _disposed = true;
            
            // Limpiar archivo temporal de la base de datos
            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                    Console.WriteLine($"🗑️ Archivo temporal eliminado: {_databasePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo eliminar archivo temporal {_databasePath}: {ex.Message}");
            }
        }
        
        base.Dispose(disposing);
    }

    /// <summary>
    /// Limpia todos los datos de la base de datos para el test actual
    /// </summary>
    public void CleanupDatabase()
    {
        try
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            
            // Limpiar todas las tablas en orden correcto (respetando foreign keys)
            var tables = new[]
            {
                "RecetasIngredientes", "Recetas", "Productos", "Ingredientes",
                "MovimientosInventario", "OrdenesCompra", "ContactosProveedor", "Proveedores",
                "Reservaciones", "Comandas", "Mesas", "Preparaciones",
                "Facturas", "Promociones", "TarjetasFidelizacion", "Clientes",
                "Notificaciones", "Usuarios"
            };
            
            foreach (var table in tables)
            {
                try
                {
                    context.Database.ExecuteSqlRaw($"DELETE FROM {table}");
                    Console.WriteLine($"🧹 Tabla {table} limpiada");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ No se pudo limpiar tabla {table}: {ex.Message}");
                }
            }
            
            context.SaveChanges();
            Console.WriteLine("✅ Base de datos limpiada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error limpiando base de datos: {ex.Message}");
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
        
        // Si es un token JWT válido (Bearer), procesarlo y devolver autenticación exitosa
        if (authHeader.StartsWith("Bearer "))
        {
            return HandleBearerSchemeAsync(authHeader);
        }
        
        // Soporte para esquema "Test" (comportamiento original)
        if (authHeader.StartsWith("Test "))
        {
            return HandleTestSchemeAsync(authHeader);
        }
        
        return Task.FromResult(AuthenticateResult.Fail("Invalid authentication scheme"));
    }

    private Task<AuthenticateResult> HandleTestSchemeAsync(string authHeader)
    {
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

        // Si el rol es específico (no el por defecto), solo usar ese rol
        // Si es el rol por defecto "Administrador", agregar todos los roles para compatibilidad
        if (role == "Administrador" || role == "-Administrador") // Manejar el bug del parsing
        {
            // Agregar múltiples roles para cubrir todos los endpoints (comportamiento original)
            var allRoles = new[] { "Administrador", "Cajero", "Gerente", "Mesero", "Cocinero", "Empleado", "Chef", "SuperAdministrador" };
            foreach (var r in allRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }
        }

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
            
            // 🔧 PARSER BÁSICO DE JWT PARA TESTS
            // En tests, vamos a extraer información básica del token JWT sin validación criptográfica
            var tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
            {
                Console.WriteLine($"🔍 TestAuthenticationHandler: Token JWT inválido - formato incorrecto");
                return Task.FromResult(AuthenticateResult.Fail("Invalid JWT format"));
            }

            // Decodificar el payload (segunda parte del token)
            var payload = tokenParts[1];
            
            // Agregar padding si es necesario para Base64
            var padding = 4 - (payload.Length % 4);
            if (padding != 4)
            {
                payload += new string('=', padding);
            }
            
            // Reemplazar caracteres URL-safe
            payload = payload.Replace('-', '+').Replace('_', '/');
            
            try
            {
                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
                
                // Parsear el JSON del payload
                using var jsonDoc = JsonDocument.Parse(payloadJson);
                var root = jsonDoc.RootElement;
                
                // Extraer claims del payload JWT
                var claims = new List<Claim>();
                
                // User ID (sub o uid)
                if (root.TryGetProperty("sub", out var subElement))
                {
                    var userId = subElement.GetString();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                        claims.Add(new Claim("sub", userId));
                    }
                }
                
                // User Name (name)
                if (root.TryGetProperty("name", out var nameElement))
                {
                    var userName = nameElement.GetString();
                    if (!string.IsNullOrEmpty(userName))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, userName));
                    }
                }
                
                // Email
                if (root.TryGetProperty("email", out var emailElement))
                {
                    var email = emailElement.GetString();
                    if (!string.IsNullOrEmpty(email))
                    {
                        claims.Add(new Claim(ClaimTypes.Email, email));
                    }
                }
                
                // Roles (role o roles)
                if (root.TryGetProperty("role", out var roleElement))
                {
                    var role = roleElement.GetString();
                    if (!string.IsNullOrEmpty(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
                else if (root.TryGetProperty("roles", out var rolesElement))
                {
                    if (rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            if (role.ValueKind == JsonValueKind.String)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role.GetString()!));
                            }
                        }
                    }
                }
                
                // Asegurar que tenemos al menos los claims básicos
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    var fallbackUserId = Guid.NewGuid().ToString();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, fallbackUserId));
                    claims.Add(new Claim("sub", fallbackUserId));
                }
                
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Name, "JWTUser"));
                }
                
                if (!claims.Any(c => c.Type == ClaimTypes.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, "jwt@test.com"));
                }
                
                if (!claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Empleado"));
                }

                var identity = new ClaimsIdentity(claims, "Bearer");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "Bearer");

                var extractedUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                
                Console.WriteLine($"🔍 TestAuthenticationHandler: Token JWT procesado exitosamente - UserId: {extractedUserId}, Roles: {string.Join(", ", roles)}");
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔍 TestAuthenticationHandler: Error decodificando payload JWT: {ex.Message}");
                // Continuar con fallback
            }
            
            // Fallback: simular claims básicos si no se puede procesar el token
            var fallbackClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "JWTUser"),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "jwt@test.com"),
                new Claim(ClaimTypes.Role, "Empleado"), // Rol por defecto para tokens JWT
                new Claim("sub", Guid.NewGuid().ToString()), // Subject claim
                new Claim("uid", Guid.NewGuid().ToString()) // User ID claim
            };

            var fallbackIdentity = new ClaimsIdentity(fallbackClaims, "Bearer");
            var fallbackPrincipal = new ClaimsPrincipal(fallbackIdentity);
            var fallbackTicket = new AuthenticationTicket(fallbackPrincipal, "Bearer");

            Console.WriteLine($"🔍 TestAuthenticationHandler: Token JWT procesado con fallback");
            return Task.FromResult(AuthenticateResult.Success(fallbackTicket));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: Error procesando token JWT: {ex.Message}");
            return Task.FromResult(AuthenticateResult.Fail("Invalid JWT token"));
        }
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
    // Usar la fecha real del sistema para evitar problemas de validación de 90 días
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