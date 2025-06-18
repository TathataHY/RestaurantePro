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

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase
{
    [Collection("DatabaseCollection")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly ITestOutputHelper _output;
        protected readonly DatabaseFixture _fixture;
        private IServiceScope _scope;
        protected IServiceProvider ServiceProvider;
        protected TestDbContext DbContext;

        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
            var services = new ServiceCollection();

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

            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseSqlite(_fixture.Connection);
                options.AddInterceptors(auditableEntityInterceptor, softDeleteInterceptor);
            });
            
            services.AddIdentity<IdentityApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<TestDbContext>();
            
            services.AddScoped<RestauranteProDbContext>(provider => provider.GetRequiredService<TestDbContext>());
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<TestDbContext>());
            
            services.AddSingleton(currentUserServiceMock.Object);
            services.AddSingleton(dateTimeServiceMock.Object);
            services.AddSingleton(domainEventDispatcherMock.Object);
            services.AddSingleton(auditableEntityInterceptor);
            services.AddSingleton(softDeleteInterceptor);

            services.AddSingleton(Mock.Of<ILogger<UsuarioRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProveedorRepository>>());
            services.AddSingleton(Mock.Of<ILogger<NotificacionRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProductoCategoriaRepository>>());
            services.AddSingleton(Mock.Of<ILogger<OrdenCompraRepository>>());
            services.AddSingleton(Mock.Of<ILogger<ProductoRepository>>());
            services.AddSingleton(Mock.Of<ILogger<UnitOfWork>>());
            services.AddSingleton(Mock.Of<ILogger<SoftDeleteInterceptor>>());

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<JwtSecurityTokenHandler>();

            services.Configure<JwtConfiguration>(options =>
            {
                options.Secret = "TestSuperSecretKeyForJwtTokenGenerationLongEnough";
                options.Issuer = "test.issuer.com";
                options.Audience = "test.audience.com";
                options.ExpirationInMinutes = 60;
                options.RefreshTokenExpirationInDays = 7;
            });
            
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            
            services.AddScoped<IUnitOfWork>(provider => 
                new UnitOfWork(provider.GetRequiredService<TestDbContext>(), 
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

            var serviceProviderFactory = services.BuildServiceProvider();
            _scope = serviceProviderFactory.CreateScope();
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