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
            
            // 🔧 REGISTRAR EL SERVICIO DE SEED DE DATOS
            services.AddScoped<ISeedDataService, TestSeedDataService>();
            
            // 🔧 REGISTRAR SERVICIOS MOCK ADICIONALES PARA EVENT HANDLERS
            services.AddScoped<RestaurantePro.Application.Common.Interfaces.ISMSService, MockSMSService>();

            // 🔧 REGISTRAR CACHE FAKE PARA HANDLERS QUE LO REQUIEREN
            services.AddSingleton<RestaurantePro.Domain.Core.SharedKernel.Services.Cache.ICacheService, TestCacheService>();
            
            // 🔧 REGISTRAR TEST ANALYTICS SERVICE
            Console.WriteLine("🔧 Registrando TestAnalyticsService en MobileIntegrationTestFixture");
            services.AddScoped<RestaurantePro.Domain.Core.Analytics.Interfaces.IAnalyticsService, TestAnalyticsService>();
            
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
/// Implementación de prueba del servicio de analytics para tests móviles
/// </summary>
public class TestAnalyticsService : RestaurantePro.Domain.Core.Analytics.Interfaces.IAnalyticsService
{
    public TestAnalyticsService()
    {
        Console.WriteLine("🔧 TestAnalyticsService constructor llamado en MobileIntegrationTestFixture");
    }

    public async Task<RestaurantePro.Domain.Core.Analytics.DTOs.MetricasDiaDto> ObtenerMetricasDiaAsync()
    {
        Console.WriteLine("🔧 TestAnalyticsService.ObtenerMetricasDiaAsync() llamado en MobileIntegrationTestFixture");
        Console.WriteLine("🔧 Retornando datos mock para ObtenerMetricasDiaAsync");
        return await Task.FromResult(new RestaurantePro.Domain.Core.Analytics.DTOs.MetricasDiaDto
        {
            Fecha = DateTime.Today,
            TotalVentas = 1250.50m,
            TotalComandas = 15,
            TotalProductosVendidos = 45,
            TiempoPromedioPreparacion = 12,
            PorcentajeOcupacionMesas = 75.5m,
            ClientesAtendidos = 42,
            TopProductos = new List<RestaurantePro.Domain.Core.Analytics.DTOs.TopProductoDto>
            {
                new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa Clásica", Categoria = "Platos Principales", CantidadVendida = 8, TotalVentas = 320.00m, PorcentajeTotalVentas = 25.6m, PrecioPromedio = 40.00m }
            }
        });
    }

    public async Task<RestaurantePro.Domain.Core.Analytics.DTOs.MetricasRangoDto> ObtenerMetricasRangoAsync(DateTime fechaDesde, DateTime fechaHasta)
    {
        return await Task.FromResult(new RestaurantePro.Domain.Core.Analytics.DTOs.MetricasRangoDto
        {
            FechaDesde = fechaDesde,
            FechaHasta = fechaHasta,
            TotalVentas = 8750.75m,
            TotalComandas = 105,
            PromedioVentasDiarias = 1250.11m,
            PromedioComandasDiarias = 15.0m,
            MetricasPorDia = new List<RestaurantePro.Domain.Core.Analytics.DTOs.MetricasDiaDto>()
        });
    }

    public async Task<List<RestaurantePro.Domain.Core.Analytics.DTOs.TopProductoDto>> ObtenerTopProductosAsync(int limite, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        return await Task.FromResult(new List<RestaurantePro.Domain.Core.Analytics.DTOs.TopProductoDto>
        {
            new() { ProductoId = Guid.NewGuid(), NombreProducto = "Hamburguesa Clásica", Categoria = "Platos Principales", CantidadVendida = 45, TotalVentas = 1800.00m, PorcentajeTotalVentas = 18.5m, PrecioPromedio = 40.00m }
        });
    }

    public async Task<RestaurantePro.Domain.Core.Analytics.DTOs.OcupacionMesasDto> ObtenerOcupacionMesasAsync(DateTime fecha)
    {
        return await Task.FromResult(new RestaurantePro.Domain.Core.Analytics.DTOs.OcupacionMesasDto
        {
            Fecha = fecha,
            TotalMesas = 20,
            MesasOcupadas = 15,
            MesasDisponibles = 5,
            TiempoPromedioOcupacion = 85,
            RotacionesMesas = 8
        });
    }

    public async Task<RestaurantePro.Domain.Core.Analytics.DTOs.TiempoPreparacionDto> ObtenerTiempoPreparacionAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
    {
        return await Task.FromResult(new RestaurantePro.Domain.Core.Analytics.DTOs.TiempoPreparacionDto
        {
            TiempoPromedioMinutos = 12,
            TiempoMinimoMinutos = 5,
            TiempoMaximoMinutos = 25,
            TotalPreparaciones = 150,
            PreparacionesEnTiempo = 135,
            PreparacionesFueraTiempo = 15,
            TiempoEstandarMinutos = 15
        });
    }

    public async Task<List<RestaurantePro.Domain.Core.Analytics.DTOs.VentasHoraDto>> ObtenerVentasPorHoraAsync(DateTime fecha)
    {
        return await Task.FromResult(new List<RestaurantePro.Domain.Core.Analytics.DTOs.VentasHoraDto>
        {
            new() { Hora = 12, TotalVentas = 450.00m, NumeroComandas = 8, PorcentajeTotalVentas = 36.0m },
            new() { Hora = 13, TotalVentas = 650.00m, NumeroComandas = 12, PorcentajeTotalVentas = 52.0m },
            new() { Hora = 14, TotalVentas = 350.00m, NumeroComandas = 6, PorcentajeTotalVentas = 28.0m }
        });
    }
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
/// Implementación de prueba del servicio de API para tests móviles
/// </summary>
public class TestApiService : RestaurantePro.Mobile.Core.Services.Api.IApiService
{
    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> GetAsync<T>(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        // Mock response para tests
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>
        {
            Success = true,
            Data = default(T),
            Message = "Mock response"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PostAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>
        {
            Success = true,
            Data = default(T),
            Message = "Mock response"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PutAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>
        {
            Success = true,
            Data = default(T),
            Message = "Mock response"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>> PatchAsync<T>(string endpoint, object data, string? token = null, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<T>
        {
            Success = true,
            Data = default(T),
            Message = "Mock response"
        };
        return Task.FromResult(response);
    }

    public Task<RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>> DeleteAsync(string endpoint, string? token = null, CancellationToken cancellationToken = default)
    {
        var response = new RestaurantePro.Mobile.Core.Models.DTOs.ApiResponse<bool>
        {
            Success = true,
            Data = true,
            Message = "Mock response"
        };
        return Task.FromResult(response);
    }
}

/// <summary>
/// Implementación de prueba del servicio de autenticación para tests móviles
/// </summary>
public class TestAuthService : RestaurantePro.Mobile.Core.Services.Authentication.IAuthService
{
    public Task<string?> GetTokenAsync()
    {
        return Task.FromResult<string?>("mock-token");
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
                Token = "mock-token",
                RefreshToken = "mock-refresh-token",
                User = new RestaurantePro.Mobile.Core.Models.DTOs.AuthUser
                {
                    Id = 1,
                    Email = email,
                    Nombre = "Test",
                    Apellido = "User",
                    Roles = new List<string> { "Mesero" }
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
            Email = "test@example.com",
            Nombre = "Test",
            Apellido = "User",
            Roles = new List<string> { "Mesero" }
        };
        return Task.FromResult<RestaurantePro.Mobile.Core.Models.DTOs.AuthUser?>(user);
    }

    public Task<string?> GetUserIdAsync()
    {
        return Task.FromResult<string?>("1");
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