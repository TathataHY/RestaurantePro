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
            
            // 🔧 REGISTRAR TEST ANALYTICS SERVICE
            Console.WriteLine("🔧 Registrando TestAnalyticsService en MobileIntegrationTestFixture");
            services.AddScoped<RestaurantePro.Domain.Core.Analytics.Interfaces.IAnalyticsService, TestAnalyticsService>();
            
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