using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Comercial.Services;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.SharedKernel;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.Base.Events;
using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Core.Base.Events.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Infrastructure.DependencyInjection;
using System.Text.Json;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Identity;
using RestaurantePro.Api.Hubs;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.ExternalServices.FileStorage;
using RestaurantePro.Infrastructure.ExternalServices.Payment;
using RestaurantePro.Infrastructure.ExternalServices.SMS;
using RestaurantePro.Infrastructure.Caching;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Infrastructure.Persistence;
using RestaurantePro.Infrastructure.Identity.Configuration;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Services;

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

    protected override IHost CreateHost(IHostBuilder builder)
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
            // 🔧 DESHABILITAR VALIDACIÓN DE SERVICIOS TEMPORALMENTE PARA DEBUGGING
            services.Configure<ServiceProviderOptions>(options =>
            {
                options.ValidateScopes = false;
                options.ValidateOnBuild = false;
            });

            // 🔧 CONFIGURACIÓN DE BASE DE DATOS TEMPORAL ÚNICA POR TEST
            Console.WriteLine($"🗄️ Configurando BD temporal: {_databasePath}");
            
            // 🔧 CONFIGURAR CADENA DE CONEXIÓN SQLITE ANTES DE REGISTRAR SERVICIOS
            // Esto asegura que todos los contextos (incluidos los secundarios) usen SQLite
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", _connectionString},
                    {"ConnectionStrings:CoreConnection", _connectionString},
                    {"ConnectionStrings:ComercialConnection", _connectionString},
                    {"ConnectionStrings:OperacionesConnection", _connectionString},
                    {"ConnectionStrings:InventarioConnection", _connectionString},
                    {"ConnectionStrings:ProveedoresConnection", _connectionString},
                    {"Jwt:SecretKey", "test-secret-key-for-integration-tests-only"},
                    {"Jwt:Issuer", "test-issuer"},
                    {"Jwt:Audience", "test-audience"},
                    {"Jwt:ExpirationMinutes", "60"}
                });

            var config = configBuilder.Build();

            // 🔧 CONFIGURAR IDENTITY SIN JWT BEARER PARA TESTS
            ConfigureIdentityForTests(services, config);

            // 🔧 RE-REGISTRAR AUTENTICACIÓN PARA FORZAR EL HANDLER DE TEST COMO ESQUEMA POR DEFECTO
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

            // 🚀 CONFIGURAR SIGNALR PARA TESTS
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
                options.MaximumParallelInvocationsPerClient = 1;
            });

            // 🚀 REGISTRAR SERVICIOS DE SIGNALR PARA TESTS
            services.AddScoped<ISignalRHub, RestaurantePro.Api.Services.SignalRHubService>();

            // 🔧 CONFIGURAR INFRAESTRUCTURA PARA TESTS SIN AUTENTICACIÓN JWT
            // Registrar servicios de persistencia
            services.AddPersistenceServices(config, isTestEnvironment: true);
            
            // Registrar IDelayProvider (necesario para servicios y handlers)
            services.AddSingleton<IDelayProvider, DelayProvider>();
            
            // Registrar ITimeProvider (necesario para PerformanceBehavior)
            services.AddSingleton<ITimeProvider, SystemTimeProvider>();
            
            // Registrar servicios externos
            services.AddExternalServices(config);
            
            // Registrar servicios de caché
            services.AddCachingServices(config);
            
            // Registrar servicios de logging
            services.AddLoggingServices(config);
            
            // 🚀 Registrar servicios de SignalR
            services.AddScoped<ISignalRService, SignalRService>();
            services.AddSingleton<IHubConnectionManager, HubConnectionManager>();

            // 🔧 SOBRESCRIBIR CONFIGURACIÓN DE BASE DE DATOS PARA TESTS (SQLite en lugar de SQL Server)
            // Remover configuración existente de DbContext
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // 🔧 REMOVER SQL SERVER EXPLÍCITAMENTE PARA EVITAR CONFLICTOS
            var sqlServerDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RestauranteProDbContext>) && 
                d.ImplementationType?.Name.Contains("SqlServer") == true);
            if (sqlServerDescriptor != null)
            {
                services.Remove(sqlServerDescriptor);
            }

            // Agregar configuración SQLite para tests
            services.AddDbContext<RestauranteProDbContext>(options =>
            {
                options.UseSqlite(_connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                
                // Configurar interceptores para que los eventos de dominio se procesen
                var serviceProvider = services.BuildServiceProvider();
                var domainEventInterceptor = serviceProvider.GetRequiredService<DomainEventInterceptor>();
                var auditableEntityInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
                var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();
                
                options.AddInterceptors(domainEventInterceptor);
                options.AddInterceptors(auditableEntityInterceptor);
                options.AddInterceptors(softDeleteInterceptor);
            });

            // 🔧 CONFIGURAR ASP.NET CORE IDENTITY PARA TESTS
            // (El registro de Identity se realiza en AddInfrastructureServices, no es necesario aquí)

            // 🔧 CONFIGURAR CONTROLADORES PARA TESTS
            services.AddControllers().AddApplicationPart(typeof(RestaurantePro.Api.Controllers.Core.AuthController).Assembly);
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // 🔧 CONFIGURAR SERVICIOS DE APLICACIÓN PARA TESTS
            // services.AddApplicationServices();
            services.AddSingleton<IIdentityService, FakeIdentityService>();
            services.AddScoped<IJwtTokenService, FakeJwtTokenService>();
            services.AddScoped<IUserPermissionService, FakeUserPermissionService>();
            services.AddScoped<ICurrentUserService, TestCurrentUserService>();
            services.AddScoped<INotificationService, TestNotificationService>();
            // Usar el DomainEventDispatcher real para que los eventos se procesen correctamente
            services.AddScoped<IDomainEventDispatcher, RestaurantePro.Domain.Core.Base.Events.Dispatcher.DomainEventDispatcher>();
            
            // 🔧 REGISTRAR FAKE DATETIME SERVICE PARA TESTS
            services.AddSingleton<IDateTimeService, FakeDateTimeService>();

            // 🔧 CONFIGURAR SERVICIOS DE INFRAESTRUCTURA PARA TESTS
            // services.AddScoped<IEmailService, FakeEmailService>();
            // Comentar servicios complejos temporalmente para enfocarse en SignalR
            // services.AddScoped<IFileStorageService, FakeFileStorageService>();
            // services.AddScoped<IPaymentService, FakePaymentService>();
            // services.AddScoped<ISMSService, FakeSmsService>();
            // services.AddScoped<ICacheService, FakeCacheService>();
            // services.AddScoped<ISignalRService, FakeSignalRService>();

            // 🔧 CONFIGURAR REPOSITORIOS PARA TESTS
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<IMesaRepository, MesaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            // 🔧 CONFIGURAR SEED DE USUARIO DE PRUEBA
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
                
                // 🔧 CREAR BASE DE DATOS PARA TESTS (sin migraciones)
                Console.WriteLine("🗄️ Creando esquema de base de datos temporal...");
                dbContext.Database.EnsureCreated();
                Console.WriteLine("✅ Esquema de base de datos creado correctamente");
                
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                
                // Crear rol si no existe
                var adminRole = roleManager.FindByNameAsync("Administrador").GetAwaiter().GetResult();
                if (adminRole == null)
                {
                    roleManager.CreateAsync(new ApplicationRole {
                        Name = "Administrador",
                        Description = "Rol de administrador para pruebas",
                        CreatedOn = DateTime.UtcNow,
                        IsSystemRole = true
                    }).GetAwaiter().GetResult();
                    Console.WriteLine("✅ Rol Administrador creado");
                }

                // Crear usuario de prueba
                var email = "admin@restaurantepro.com";
                var existingUser = userManager.FindByEmailAsync(email).GetAwaiter().GetResult();
                if (existingUser == null)
                {
                    var user = new RestaurantePro.Infrastructure.Identity.Models.ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        Nombre = "Admin Test",
                        Apellidos = "Test",
                        FotoPerfil = "",
                        RefreshToken = "",
                        FechaCreacion = DateTime.UtcNow,
                        Activo = true
                    };
                    var result = userManager.CreateAsync(user, "Admin123!").GetAwaiter().GetResult();
                    if (result.Succeeded)
                    {
                        userManager.AddToRoleAsync(user, "Administrador").GetAwaiter().GetResult();
                        Console.WriteLine($"✅ Usuario de prueba creado: {email}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Error creando usuario de prueba: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"ℹ️ Usuario de prueba ya existe: {email}");
                }
            }
        });

        // 🔧 CONFIGURAR PIPELINE DE LA APLICACIÓN PARA TESTS
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseUrls("http://localhost:0"); // Puerto dinámico para evitar conflictos
            
            webHostBuilder.Configure(app =>
            {
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    
                    // 🚀 MAPEAR SIGNALR HUBS PARA TESTS
                    endpoints.MapHub<ComandaHub>("/hubs/comandas");
                    endpoints.MapHub<NotificationHub>("/hubs/notifications");
                    endpoints.MapHub<InventarioHub>("/hubs/inventario");
                });
            });
        });

        return base.CreateHost(builder);
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
    /// <summary>
    /// Configura Identity para tests sin JWT Bearer
    /// </summary>
    private static void ConfigureIdentityForTests(IServiceCollection services, IConfiguration configuration)
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

        // Configurar cookies para que devuelvan 401/403 en vez de 302 en APIs
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // 🔧 NO CONFIGURAR JWT BEARER EN TESTS - usar TestAuthenticationHandler en su lugar
        // La autenticación ya está configurada arriba con el esquema "Test"

        // Registrar servicios de Identity
        services.AddScoped<IJwtTokenService, FakeJwtTokenService>();
        services.AddScoped<IIdentityService, FakeIdentityService>();
        services.AddScoped<IUserPermissionService, FakeUserPermissionService>();
    }

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
        Console.WriteLine($"🔍 TestAuthenticationHandler: Iniciando autenticación...");
        Console.WriteLine($"🔍 TestAuthenticationHandler: URL: {Request.Path}");
        Console.WriteLine($"🔍 TestAuthenticationHandler: Método: {Request.Method}");
        Console.WriteLine($"🔍 TestAuthenticationHandler: Headers disponibles: {string.Join(", ", Request.Headers.Keys)}");
        
        // Verificar si hay un header de autorización
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: NO hay header Authorization");
            return Task.FromResult(AuthenticateResult.Fail("No authorization header"));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        Console.WriteLine($"🔍 TestAuthenticationHandler: Header recibido: {authHeader}");
        
        // Si es un token JWT válido (Bearer), procesarlo y devolver autenticación exitosa
        if (authHeader.StartsWith("Bearer "))
        {
            Console.WriteLine($"🔍 TestAuthenticationHandler: Procesando Bearer token");
            return HandleBearerSchemeAsync(authHeader);
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
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "TestUser"),
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
            
            var payloadBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Payload JSON: {payloadJson}");
            
            // Parsear el JSON del payload
            var payloadData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(payloadJson);
            
            // Extraer claims del payload
            var claims = new List<Claim>();
            
            if (payloadData.TryGetProperty("sub", out var sub))
            {
                var userId = sub.GetString() ?? "fake-user-id";
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                claims.Add(new Claim("sub", userId));
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - UserId extraído: {userId}");
            }
            
            if (payloadData.TryGetProperty("name", out var name))
            {
                var userName = name.GetString() ?? "FakeUser";
                claims.Add(new Claim(ClaimTypes.Name, userName));
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - UserName extraído: {userName}");
            }
            
            if (payloadData.TryGetProperty("email", out var email))
            {
                var userEmail = email.GetString() ?? "fake@test.com";
                claims.Add(new Claim(ClaimTypes.Email, userEmail));
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Email extraído: {userEmail}");
            }
            
            if (payloadData.TryGetProperty("role", out var role))
            {
                var userRole = role.GetString() ?? "Empleado";
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Role extraído: {userRole}");
            }
            
            if (payloadData.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
            {
                foreach (var roleItem in roles.EnumerateArray())
                {
                    var userRole = roleItem.GetString() ?? "Empleado";
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Role adicional extraído: {userRole}");
                }
            }
            
            // Agregar permisos básicos para tests
            claims.Add(new Claim("permission", "perfil.read"));
            claims.Add(new Claim("permission", "perfil.update"));
            
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Claims finales: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");
            
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] HandleBearerSchemeAsync - Error parseando JWT: {ex.Message}");
            // Si hay cualquier error, aceptar el token como válido para tests
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
/// Mock simple de ISignalRHub para tests
/// </summary>
public class MockSignalRHub : ISignalRHub
{
    public Task SendToGroupAsync(string groupName, string method, params object[] args)
    {
        // Mock simple - no hace nada en tests
        return Task.CompletedTask;
    }

    public Task SendToAllAsync(string method, params object[] args)
    {
        // Mock simple - no hace nada en tests
        return Task.CompletedTask;
    }

    public Task SendToUserAsync(string userId, string method, params object[] args)
    {
        // Mock simple - no hace nada en tests
        return Task.CompletedTask;
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
    private readonly Dictionary<string, (string Password, string UserId, string UserName, List<string> Roles)> _users = new();
    private readonly Dictionary<string, string> _registeredUsers = new();

    public FakeIdentityService()
    {
        // Usuario admin por defecto para tests (contraseña debe coincidir con IdentityUsersSeeder)
        _users["admin@restaurantepro.com"] = ("AdminRestaurante123!", "11111111-1111-1111-1111-111111111111", "admin", new List<string> { "Administrador" });
    }

    // 🔧 MÉTODO PRIVADO PARA GENERAR JWT REAL
    private string GenerateJwtToken(string userId, string userName, string email, IList<string> roles)
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        var payload = new
        {
            sub = userId,
            name = userName,
            email = email,
            role = roles.FirstOrDefault() ?? "Empleado",
            roles = roles,
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        // Para tests, usamos una firma dummy
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-signature"))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        return $"{header}.{payloadBase64}.{signature}";
    }

    public Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
        => Task.FromResult((Result.Success(), "fake-user-id"));
    
    public Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
    {
        // 🔧 VALIDACIONES REALISTAS PARA TESTS
        var errors = new List<string>();

        // Validar campos requeridos
        if (string.IsNullOrWhiteSpace(nombre))
            errors.Add("El nombre es requerido");
        if (string.IsNullOrWhiteSpace(apellidos))
            errors.Add("Los apellidos son requeridos");
        if (string.IsNullOrWhiteSpace(email))
            errors.Add("El email es requerido");
        if (string.IsNullOrWhiteSpace(username))
            errors.Add("El username es requerido");
        if (string.IsNullOrWhiteSpace(password))
            errors.Add("La contraseña es requerida");
        if (string.IsNullOrWhiteSpace(rol))
            errors.Add("El rol es requerido");

        // Validar formato de email
        if (!string.IsNullOrWhiteSpace(email) && !email.Contains("@"))
            errors.Add("El formato del email no es válido");

        // Validar fortaleza de contraseña
        if (!string.IsNullOrWhiteSpace(password))
        {
            if (password.Length < 6)
                errors.Add("La contraseña debe tener al menos 6 caracteres");
            else if (password == "123" || password == "password" || password == "qwerty" || 
                     password == "aaaaaa" || password == "123456789")
                errors.Add("La contraseña es demasiado débil");
        }

        // Si hay errores de validación, devolver failure
        if (errors.Any())
        {
            return Task.FromResult(Result.Failure<string>(string.Join("; ", errors)));
        }

        // Validar duplicados
        if (_users.ContainsKey(email))
        {
            return Task.FromResult(Result.Failure<string>("El email ya está registrado"));
        }

        // Si todo está bien, registrar el usuario
        var userId = Guid.NewGuid().ToString();
        _users[email] = (password, userId, username, new List<string> { rol });
        _registeredUsers[email] = userId;

        return Task.FromResult(Result.Success(userId));
    }
    
    public Task<Result> CreateRoleAsync(string roleName, string description, bool isSystemRole)
        => Task.FromResult(Result.Success());
    
    public Task<AuthResponse> LoginAsync(string email, string password)
    {
        if (_users.TryGetValue(email, out var user) && user.Password == password)
        {
            var token = GenerateJwtToken(user.UserId, user.UserName, email, user.Roles);
            return Task.FromResult(new AuthResponse 
            { 
                Success = true, 
                Message = "OK", 
                Token = token, 
                Expiration = DateTime.UtcNow.AddHours(1), 
                UserId = user.UserId, 
                UserName = user.UserName, 
                Roles = user.Roles 
            });
        }
        
        return Task.FromResult(new AuthResponse { Success = false, Message = "Credenciales inválidas" });
    }
    
    public Task<List<UserDto>> GetUsersAsync()
    {
        var users = _users.Select(kvp => new UserDto 
        { 
            Id = kvp.Value.UserId, 
            UserName = kvp.Value.UserName, 
            Email = kvp.Key, 
            EmailConfirmed = true, 
            Roles = kvp.Value.Roles 
        }).ToList();
        
        return Task.FromResult(users);
    }
    
    public Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = _users.FirstOrDefault(kvp => kvp.Value.UserId == userId);
        if (user.Key != null)
        {
            return Task.FromResult(new UserDto 
            { 
                Id = user.Value.UserId, 
                UserName = user.Value.UserName, 
                Email = user.Key, 
                EmailConfirmed = true, 
                Roles = user.Value.Roles 
            });
        }
        
        return Task.FromResult(new UserDto { Id = userId, UserName = "FakeUser", Email = "fake@email.com", EmailConfirmed = true, Roles = new List<string> { "Admin" } });
    }
    
    public Task<Result> UpdateUserAsync(string id, string nombre, string apellidos, string email, string username)
        => Task.FromResult(Result.Success());
    
    public Task<Result> DeleteUserAsync(string userId)
        => Task.FromResult(Result.Success());
    
    public Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? confirmNewPassword = null)
    {
        // 🔧 DEBUG: Log para identificar el problema
        Console.WriteLine($"[DEBUG] ChangePasswordAsync - userId: {userId}");
        Console.WriteLine($"[DEBUG] ChangePasswordAsync - currentPassword: {currentPassword}");
        Console.WriteLine($"[DEBUG] ChangePasswordAsync - newPassword: {newPassword}");
        Console.WriteLine($"[DEBUG] ChangePasswordAsync - confirmNewPassword: {confirmNewPassword}");
        Console.WriteLine($"[DEBUG] ChangePasswordAsync - _users count: {_users.Count}");
        foreach (var user in _users)
        {
            Console.WriteLine($"[DEBUG] User: {user.Key} -> UserId: {user.Value.UserId}, Password: {user.Value.Password}");
        }
        
        // Buscar el usuario por userId
        var userEntry = _users.FirstOrDefault(kvp => kvp.Value.UserId == userId);
        if (userEntry.Key == null)
        {
            Console.WriteLine($"[DEBUG] Usuario no encontrado para userId: {userId}");
            return Task.FromResult(Result.Failure("Usuario no encontrado"));
        }
        
        Console.WriteLine($"[DEBUG] Usuario encontrado: {userEntry.Key} con contraseña: {userEntry.Value.Password}");
        
        // Validar contraseña actual
        if (userEntry.Value.Password != currentPassword)
        {
            Console.WriteLine($"[DEBUG] Contraseña actual incorrecta. Esperada: {userEntry.Value.Password}, Recibida: {currentPassword}");
            return Task.FromResult(Result.Failure("Contraseña actual incorrecta"));
        }
        
        // Validar que la nueva contraseña sea diferente
        if (currentPassword == newPassword)
        {
            return Task.FromResult(Result.Failure("La nueva contraseña debe ser diferente a la actual"));
        }
        
        // Validar confirmación si se provee
        if (confirmNewPassword != null && newPassword != confirmNewPassword)
        {
            return Task.FromResult(Result.Failure("La confirmación de la nueva contraseña no coincide"));
        }
        
        // Actualizar contraseña
        _users[userEntry.Key] = (newPassword, userEntry.Value.UserId, userEntry.Value.UserName, userEntry.Value.Roles);
        Console.WriteLine($"[DEBUG] Contraseña actualizada exitosamente");
        return Task.FromResult(Result.Success());
    }
    
    public Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
    {
        // Validar credenciales para tests
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return Task.FromResult(Result.Failure<AuthResponse>("Email y contraseña son requeridos"));
        }

        // Verificar si el usuario existe y la contraseña es correcta
        if (_users.TryGetValue(email, out var user) && user.Password == password)
        {
            var token = GenerateJwtToken(user.UserId, user.UserName, email, user.Roles);
            return Task.FromResult(Result.Success(new AuthResponse 
            { 
                Success = true, 
                Message = "Login exitoso", 
                Token = token, 
                Expiration = DateTime.UtcNow.AddHours(1), 
                UserId = user.UserId, 
                UserName = user.UserName, 
                Roles = user.Roles 
            }));
        }

        // Para cualquier otra credencial, devolver error
        return Task.FromResult(Result.Failure<AuthResponse>("Usuario o contraseña incorrectos"));
    }
    
    public Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        => Task.FromResult(Result.Success(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } }));
}

// Fake para IJwtTokenService
public class FakeJwtTokenService : IJwtTokenService
{
    public JwtTokenResponse GenerateToken(string userId, string userName, string email, IList<string> roles)
    {
        // Generar un token JWT válido para tests
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        var payload = new
        {
            sub = userId,
            name = userName,
            email = email,
            role = roles.FirstOrDefault() ?? "Empleado",
            roles = roles,
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        // Para tests, usamos una firma dummy
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("test-signature"))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        
        var token = $"{header}.{payloadBase64}.{signature}";
        
        return new JwtTokenResponse { AccessToken = token, TokenType = "Bearer", ExpiresIn = 3600, RequiresRefresh = false };
    }
    
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
    
    // Método para avanzar el tiempo en los tests
    public void AdvanceTime(TimeSpan timeSpan) => _now = _now.Add(timeSpan);
    
    // Método para avanzar el tiempo en minutos
    public void AdvanceMinutes(int minutes) => _now = _now.AddMinutes(minutes);
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

// 🔧 SERVICIOS FAKE PARA TESTS - TEMPORALMENTE COMENTADOS PARA ENFOCARSE EN SIGNALR
/*
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Test email sent to: {To}, Subject: {Subject}", to, subject);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null)
    {
        _logger.LogInformation("Test email with attachment sent to: {To}, Subject: {Subject}", to, subject);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendHtmlEmailAsync(string to, string subject, string htmlBody)
    {
        _logger.LogInformation("Test HTML email sent to: {To}, Subject: {Subject}", to, subject);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendBulkEmailAsync(List<string> toAddresses, string subject, string body)
    {
        _logger.LogInformation("Test bulk email sent to {Count} recipients, Subject: {Subject}", toAddresses.Count, subject);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        _logger.LogInformation("Test email with attachment sent to: {To}, Subject: {Subject}", to, subject);
        return await Task.FromResult(true);
    }
}

public class FakeFileStorageService : IFileStorageService
{
    private readonly ILogger<FakeFileStorageService> _logger;

    public FakeFileStorageService(ILogger<FakeFileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SubirArchivoAsync(DatosArchivo datosArchivo, string nombreArchivo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test file uploaded: {FileName}", nombreArchivo);
        return await Task.FromResult($"fake-url/{nombreArchivo}");
    }

    public async Task<Stream> DescargarArchivoAsync(string urlArchivo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test file downloaded: {FileUrl}", urlArchivo);
        return await Task.FromResult(Stream.Null);
    }

    public async Task<bool> EliminarArchivoAsync(string urlArchivo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test file deleted: {FileUrl}", urlArchivo);
        return await Task.FromResult(true);
    }

    public async Task<string> ObtenerUrlTemporalAsync(string urlArchivo, TimeSpan duracion, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test temporary URL generated: {FileUrl}", urlArchivo);
        return await Task.FromResult($"fake-temp-url/{urlArchivo}");
    }
}

public class FakePaymentService : IPaymentService
{
    private readonly ILogger<FakePaymentService> _logger;

    public FakePaymentService(ILogger<FakePaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<PaymentResult>> ProcesarPagoAsync(decimal monto, string moneda, string descripcion, Dictionary<string, string>? metadatos = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test payment processed: {Amount} {Currency}", monto, moneda);
        return await Task.FromResult(Result.Success(new PaymentResult
        {
            TransaccionId = Guid.NewGuid().ToString(),
            Estado = PaymentStatus.Completado,
            Monto = monto,
            Moneda = moneda,
            ReferenciaComercio = "test-ref",
            Mensaje = "Test payment successful",
            FechaTransaccion = DateTime.UtcNow
        }));
    }

    public async Task<Result<RefundResult>> ReembolsarPagoAsync(string transaccionId, decimal? montoReembolso = null, string? motivo = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test payment refunded: {TransactionId}, Amount: {Amount}", transaccionId, montoReembolso);
        return await Task.FromResult(Result.Success(new RefundResult
        {
            ReembolsoId = Guid.NewGuid().ToString(),
            TransaccionOriginalId = transaccionId,
            MontoReembolsado = montoReembolso ?? 0,
            Estado = RefundStatus.Completado,
            FechaReembolso = DateTime.UtcNow
        }));
    }

    public async Task<Result<PaymentStatus>> VerificarEstadoPagoAsync(string transaccionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test payment status check: {TransactionId}", transaccionId);
        return await Task.FromResult(Result.Success(PaymentStatus.Completado));
    }
}

public class FakeSmsService : ISMSService
{
    private readonly ILogger<FakeSmsService> _logger;

    public FakeSmsService(ILogger<FakeSmsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendSMSAsync(string phoneNumber, string message)
    {
        _logger.LogInformation("Test SMS sent to: {PhoneNumber}, Message: {Message}", phoneNumber, message);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendBulkSMSAsync(List<string> phoneNumbers, string message)
    {
        _logger.LogInformation("Test bulk SMS sent to {Count} numbers", phoneNumbers.Count);
        return await Task.FromResult(true);
    }

    public async Task<bool> SendSMSWithTrackingAsync(string phoneNumber, string message, Guid? clienteId = null, string? tipoNotificacion = null)
    {
        _logger.LogInformation("Test SMS with tracking sent to: {PhoneNumber}, Message: {Message}, ClienteId: {ClienteId}, Tipo: {Tipo}", phoneNumber, message, clienteId, tipoNotificacion);
        return await Task.FromResult(true);
    }

    public bool IsValidPhoneNumber(string phoneNumber)
    {
        _logger.LogInformation("Test phone number validation: {PhoneNumber}", phoneNumber);
        return true;
    }

    public async Task<string> GetDeliveryStatusAsync(string messageId)
    {
        _logger.LogInformation("Test SMS delivery status for message: {MessageId}", messageId);
        return await Task.FromResult("Delivered");
    }
}

public class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly ILogger<FakeCacheService> _logger;

    public FakeCacheService(ILogger<FakeCacheService> logger)
    {
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        _logger.LogInformation("Test cache get: {Key}", key);
        return _cache.TryGetValue(key, out var value) ? (T)value : default(T);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test cache get async: {Key}", key);
        return await Task.FromResult(_cache.TryGetValue(key, out var value) ? (T)value : default(T));
    }

    public bool Exists(string key)
    {
        return _cache.ContainsKey(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_cache.ContainsKey(key));
    }

    public void Set<T>(string key, T value, int expirationMinutes = 60)
    {
        _logger.LogInformation("Test cache set: {Key}", key);
        _cache[key] = value!;
    }

    public async Task SetAsync<T>(string key, T value, int expirationMinutes, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test cache set async: {Key}", key);
        _cache[key] = value!;
        await Task.CompletedTask;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test cache set async with timespan: {Key}", key);
        _cache[key] = value!;
        await Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _logger.LogInformation("Test cache remove: {Key}", key);
        _cache.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test cache remove async: {Key}", key);
        _cache.Remove(key);
        await Task.CompletedTask;
    }

    public void InvalidatePattern(string pattern)
    {
        _logger.LogInformation("Test cache invalidate pattern: {Pattern}", pattern);
        var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    public async Task InvalidatePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Test cache invalidate pattern async: {Pattern}", pattern);
        var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
        await Task.CompletedTask;
    }

    public T GetOrCreate<T>(string key, Func<T> factory, int expirationMinutes = 60)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        var newValue = factory();
        _cache[key] = newValue!;
        return newValue;
    }

    public T GetOrAdd<T>(string key, Func<T> factory, int expirationMinutes = 60)
    {
        return GetOrCreate(key, factory, expirationMinutes);
    }

    public async Task<T> GetOrAddAsync<T>(string key, Func<CancellationToken, Task<T>> factory, int expirationMinutes, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        var newValue = await factory(cancellationToken);
        _cache[key] = newValue!;
        return newValue;
    }
}

public class FakeSignalRService : ISignalRService
{
    private readonly ILogger<FakeSignalRService> _logger;

    public FakeSignalRService(ILogger<FakeSignalRService> logger)
    {
        _logger = logger;
    }

    public async Task EnviarNotificacionAUsuarioAsync(Guid usuarioId, string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("Test SignalR notification sent to user {UserId}: {Title}", usuarioId, titulo);
        await Task.CompletedTask;
    }

    public async Task EnviarNotificacionAUsuariosAsync(List<Guid> usuariosIds, string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("Test SignalR notification sent to {Count} users: {Title}", usuariosIds.Count, titulo);
        await Task.CompletedTask;
    }

    public async Task EnviarNotificacionARolAsync(string rol, string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("Test SignalR notification sent to role {Role}: {Title}", rol, titulo);
        await Task.CompletedTask;
    }

    public async Task EnviarNotificacionGlobalAsync(string titulo, string mensaje, string tipo = "Info")
    {
        _logger.LogInformation("Test SignalR global notification sent: {Title}", titulo);
        await Task.CompletedTask;
    }

    public async Task ActualizarEstadoMesaAsync(Guid mesaId, string estado, object detalles)
    {
        _logger.LogInformation("Test SignalR table status update: Table {TableId}, Status: {Status}", mesaId, estado);
        await Task.CompletedTask;
    }

    public async Task ActualizarEstadoComandaAsync(Guid comandaId, string estado, object detalles)
    {
        _logger.LogInformation("Test SignalR order status update: Order {OrderId}, Status: {Status}", comandaId, estado);
        await Task.CompletedTask;
    }

    public async Task EnviarAlertaInventarioAsync(Guid ingredienteId, string nombreIngrediente, decimal stockActual, decimal stockMinimo)
    {
        _logger.LogInformation("Test SignalR inventory alert: {IngredientName}, Stock: {Stock}", nombreIngrediente, stockActual);
        await Task.CompletedTask;
    }

    public async Task<List<Guid>> ObtenerUsuariosConectadosAsync()
    {
        return await Task.FromResult(new List<Guid>());
    }

    public async Task<bool> UsuarioEstaConectadoAsync(Guid usuarioId)
    {
        return await Task.FromResult(false);
    }

    public async Task NotificarNuevaComandaAsync(object notificacion)
    {
        _logger.LogInformation("Test SignalR new order notification");
        await Task.CompletedTask;
    }

    public async Task NotificarActualizacionComandaAsync(Guid comandaId, string estado, string? mensaje = null)
    {
        _logger.LogInformation("Test SignalR order update notification: Order {OrderId}, Status: {Status}", comandaId, estado);
        await Task.CompletedTask;
    }

    public async Task NotificarEventoSistemaAsync(string evento, object datos)
    {
        _logger.LogInformation("Test SignalR system event notification: Event {Event}", evento);
        await Task.CompletedTask;
    }

    public async Task NotificarUsuarioAsync(string usuarioId, string evento, object datos)
    {
        _logger.LogInformation("Test SignalR user notification: User {UserId}, Event {Event}", usuarioId, evento);
        await Task.CompletedTask;
    }

    public async Task NotificarGrupoAsync(string grupo, string evento, object datos)
    {
        _logger.LogInformation("Test SignalR group notification: Group {Group}, Event {Event}", grupo, evento);
        await Task.CompletedTask;
    }
}
*/