using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using System;
using Xunit;
using System.Threading.Tasks;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using Xunit.Abstractions;
using RestaurantePro.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Infrastructure.Identity.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using RestaurantePro.Infrastructure.Identity.Configuration;
using RestaurantePro.Domain.Core.Base.Testing;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using RestaurantePro.Infrastructure.DependencyInjection;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache;
using RestaurantePro.Domain.Core.SharedKernel.Services.Cache.Telemetry;
using RestaurantePro.Infrastructure.Caching.Services;
using RestaurantePro.Infrastructure.Monitoring.HealthChecks;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    [Collection("DatabaseCollection")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly ITestOutputHelper _output;
        protected readonly DatabaseFixture _fixture;
        private IServiceScope _scope;
        protected IServiceProvider ServiceProvider;
        protected TestDbContext DbContext = null!;

        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
            TestEnvironment.SetTestEnvironment(true);

            // Construir configuración en memoria para pruebas
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    // Identity Settings
                    {"IdentitySettings:PasswordSettings:RequireDigit", "true"},
                    {"IdentitySettings:PasswordSettings:RequireLowercase", "true"},
                    {"IdentitySettings:PasswordSettings:RequireUppercase", "true"},
                    {"IdentitySettings:PasswordSettings:RequireNonAlphanumeric", "true"},
                    {"IdentitySettings:PasswordSettings:RequiredLength", "8"},
                    {"IdentitySettings:PasswordSettings:RequiredUniqueChars", "1"},
                    
                    {"IdentitySettings:LockoutSettings:AllowedForNewUsers", "true"},
                    {"IdentitySettings:LockoutSettings:MaxFailedAccessAttempts", "5"},
                    {"IdentitySettings:LockoutSettings:DefaultLockoutTimeSpan", "0.00:15:00"},

                    {"IdentitySettings:UserSettings:RequireUniqueEmail", "true"},
                    {"IdentitySettings:UserSettings:RequireConfirmedEmail", "false"},
                    {"IdentitySettings:UserSettings:RequireConfirmedPhoneNumber", "false"},
                    {"IdentitySettings:UserSettings:RequireConfirmedAccount", "false"},

                    // JWT Settings
                    {"JwtSettings:Secret", "TestSuperSecretKeyForJwtTokenGenerationLongEnough"},
                    {"JwtSettings:Issuer", "test.issuer.com"},
                    {"JwtSettings:Audience", "test.audience.com"},
                    {"JwtSettings:ExpirationInMinutes", "60"},
                    {"JwtSettings:RefreshTokenExpirationInDays", "7"}
                })
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            var currentUserServiceMock = new Mock<ICurrentUserService>();
            currentUserServiceMock.Setup(s => s.UserId).Returns("test-user");

            var dateTimeServiceMock = new Mock<IDateTimeService>();
            var fixedDate = new DateTime(2025, 6, 18, 12, 0, 0, DateTimeKind.Utc);
            dateTimeServiceMock.Setup(s => s.UtcNow).Returns(fixedDate);
            dateTimeServiceMock.Setup(s => s.Now).Returns(fixedDate.ToLocalTime());

            var domainEventDispatcherMock = new Mock<IDomainEventDispatcher>();
            var loggerInterceptorMock = new Mock<ILogger<AuditableEntityInterceptor>>();
            var auditableEntityInterceptor = new AuditableEntityInterceptor(currentUserServiceMock.Object, dateTimeServiceMock.Object, loggerInterceptorMock.Object);

            var loggerSoftDeleteInterceptorMock = new Mock<ILogger<SoftDeleteInterceptor>>();
            var softDeleteInterceptor = new SoftDeleteInterceptor(
                dateTimeServiceMock.Object,
                currentUserServiceMock.Object,
                loggerSoftDeleteInterceptorMock.Object);

            services.AddSingleton(auditableEntityInterceptor);
            services.AddSingleton(softDeleteInterceptor);
            services.AddSingleton(Mock.Of<ILogger<RestauranteProDbContext>>());

            services.AddScoped(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<RestauranteProDbContext>()
                    .UseSqlite("DataSource=:memory:")
                    .AddInterceptors(
                        provider.GetRequiredService<AuditableEntityInterceptor>(),
                        provider.GetRequiredService<SoftDeleteInterceptor>());

                return new TestDbContext(
                    optionsBuilder.Options,
                    provider.GetRequiredService<ILogger<RestauranteProDbContext>>(),
                    provider.GetRequiredService<IDomainEventDispatcher>()
                );
            });
            
            services.AddScoped<RestauranteProDbContext>(provider => provider.GetRequiredService<TestDbContext>());

            // Usar el setup centralizado de Identity
            services.AddIdentityServices(configuration);
            
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDbContext>());

            services.AddSingleton(currentUserServiceMock.Object);
            services.AddSingleton(dateTimeServiceMock.Object);
            services.AddScoped<IDomainEventDispatcher, TestDomainEventDispatcher>();
            
            services.AddSingleton(Mock.Of<ILogger<UsuarioRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProveedorRepository>>());
            services.AddSingleton(Mock.Of<ILogger<NotificacionRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProductoCategoriaRepository>>());
            services.AddSingleton(Mock.Of<ILogger<OrdenCompraRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProductoRepository>>());
            services.AddSingleton(Mock.Of<ILogger<UnitOfWork>>());
            services.AddSingleton(Mock.Of<ILogger<SoftDeleteInterceptor>>());
            
            services.AddSingleton<JwtSecurityTokenHandler>();
            
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

            services.AddScoped<IUnitOfWork>(provider =>
                new UnitOfWork(
                    provider.GetRequiredService<RestauranteProDbContext>(),
                    provider.GetRequiredService<ILogger<UnitOfWork>>()));

            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
            services.AddScoped<IFacturaRepository, FacturaRepository>();
            services.AddScoped<IMesaRepository, MesaRepository>();
            services.AddScoped<IReservacionRepository, ReservacionRepository>();
            services.AddScoped<IIngredienteRepository, IngredienteRepository>();
            services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
            services.AddScoped<IComandaRepository, ComandaRepository>();
            services.AddScoped<IPreparacionRepository, PreparacionRepository>();
            services.AddScoped<IProductoCategoriaRepository, ProductoCategoriaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IProveedorRepository, ProveedorRepository>();
            services.AddScoped<IContactoProveedorRepository, ContactoProveedorRepository>();
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();

            services.AddSingleton<IDateTimeService>(dateTimeServiceMock.Object);
            services.AddSingleton<ICurrentUserService>(currentUserServiceMock.Object);

            services.AddSingleton<ICacheTelemetry, InMemoryCacheTelemetry>();
            services.AddSingleton<MemoryCacheService>();
            services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<MemoryCacheService>());

            services.AddSingleton<DatabaseHealthCheck>();
            
            // Registrar SeedDataRunner para tests
            services.AddSingleton<RestaurantePro.Infrastructure.Persistence.SeedData.Extensions.SeedDataRunner>();
            
            var serviceProvider = services.BuildServiceProvider();

            _scope = serviceProvider.CreateScope();
            ServiceProvider = _scope.ServiceProvider;
            DbContext = ServiceProvider.GetRequiredService<TestDbContext>();

            await ResetDatabaseAsync();
        }

        protected async Task ResetDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }
        
        protected void ClearTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        public virtual Task DisposeAsync()
        {
            _scope?.Dispose();
            return Task.CompletedTask;
        }
    }
} 