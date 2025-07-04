using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.Interceptors;
using RestaurantePro.Domain.Core.Base.Events.Dispatcher;
using NSubstitute;
using System.Security.Claims;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Core.Notificaciones.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
using RestaurantePro.Domain.Core.Productos.Interfaces;
using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;
using RestaurantePro.Domain.Comercial.Promociones.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Proveedores.Interfaces;
using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;
using RestaurantePro.Infrastructure.Identity.Services;
using RestaurantePro.Infrastructure.Monitoring.HealthChecks;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RestaurantePro.Infrastructure.Identity.Configuration;

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

/// <summary>
/// Clase base para tests de integración de infraestructura
/// Proporciona una base de datos SQLite in-memory única para cada test
/// </summary>
    [Collection("DatabaseCollection")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly DatabaseFixture _fixture;
    protected TestRestauranteProDbContext DbContext = null!;
    protected IServiceProvider ServiceProvider = null!;

        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
        // Crear una base de datos única para este test
        DbContext = _fixture.CreateDbContext();
        
        // Configurar servicios para este test
            var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Registrar el contexto de base de datos único para este test
        services.AddScoped(provider => DbContext);
        // Registrar el contexto de test como todos los tipos base
        services.AddScoped<TestRestauranteProDbContext>(provider => DbContext);
        services.AddScoped<RestauranteProDbContext>(provider => DbContext);
        services.AddScoped<DbContext>(provider => DbContext);
        
        // Registrar interceptores
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        
        // Registrar dispatcher de eventos de dominio
        services.AddScoped<IDomainEventDispatcher>(provider => 
            Substitute.For<IDomainEventDispatcher>());

        // 🔧 REGISTRAR UNIT OF WORK
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 🔧 REPOSITORIOS CORE
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IProductoCategoriaRepository, ProductoCategoriaRepository>();
        
        // 🔧 REPOSITORIOS COMERCIAL
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<ITarjetaFidelizacionRepository, TarjetaFidelizacionRepository>();
        services.AddScoped<IPromocionRepository, PromocionRepository>();
        
        // 🔧 REPOSITORIOS OPERACIONES
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IReservacionRepository, ReservacionRepository>();
        services.AddScoped<IPreparacionRepository, PreparacionRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        
        // 🔧 REPOSITORIOS INVENTARIO
        services.AddScoped<IIngredienteRepository, IngredienteRepository>();
        services.AddScoped<IOrdenCompraRepository, OrdenCompraRepository>();
        
        // 🔧 REPOSITORIOS PROVEEDORES
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<RestaurantePro.Domain.Proveedores.Interfaces.IContactoProveedorRepository, RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores.ContactoProveedorRepository>();
        
        // 🔧 CONFIGURACIÓN DE IDENTITY REAL
        services.AddIdentity<IdentityApplicationUser, ApplicationRole>(options =>
        {
            // Configuración de contraseñas
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Configuración de usuario
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<TestRestauranteProDbContext>()
        .AddDefaultTokenProviders();

        // 🔧 CONFIGURACIÓN DE JWT PARA TESTS
        services.Configure<RestaurantePro.Infrastructure.Identity.Configuration.JwtConfiguration>(options =>
        {
            options.Secret = "TestSecretKeyForJwtTokenGenerationInIntegrationTests123456789";
            options.Issuer = "RestaurantePro.Test";
            options.Audience = "RestaurantePro.Test";
            options.ExpirationInMinutes = 60;
            options.RefreshTokenExpirationInDays = 7;
        });

        // 🔧 SERVICIOS DE IDENTITY REALES
        services.AddScoped<IIdentityService, RestaurantePro.Infrastructure.Identity.Services.IdentityService>();
        services.AddScoped<IJwtTokenService, RestaurantePro.Infrastructure.Identity.Services.JwtTokenService>();
        services.AddScoped<IUserPermissionService, RestaurantePro.Infrastructure.Identity.Services.PermissionService>();
        
        // 🔧 SERVICIOS DE MONITORING
        services.AddScoped<RestaurantePro.Infrastructure.Monitoring.HealthChecks.DatabaseHealthCheck>();
        
        // Servicios de dominio y utilidades
        services.AddScoped<IDateTimeService, FakeDateTimeService>();
        
        // Registrar loggers necesarios para los repositorios
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // 🔧 SEED DATA
        services.AddScoped<RestaurantePro.Infrastructure.Persistence.SeedData.Extensions.SeedDataRunner>();
    }

    /// <summary>
    /// Limpia la base de datos después de cada test
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        try
        {
            // Método principal: Recrear la base de datos completa
            // Esto es más rápido y confiable que intentar limpiar tablas una por una
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            
            Console.WriteLine("✅ Base de datos recreada exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al recrear base de datos: {ex.Message}");
            
            // Fallback: Intentar limpieza manual si la recreación falla
            try
            {
                await CleanupDatabaseManually();
            }
            catch (Exception cleanupEx)
            {
                Console.WriteLine($"❌ Error en limpieza manual: {cleanupEx.Message}");
                // Si ambos métodos fallan, no hacer nada más
                // Los tests continuarán con el estado actual de la BD
            }
        }
    }

    private async Task CleanupDatabaseManually()
    {
        // Solo usar este método como fallback
        // Limpiar en orden de dependencia (hijos primero, padres después)
        
        // 1. Limpiar entidades hijas (más dependientes)
        DbContext.ItemsComanda.RemoveRange(DbContext.ItemsComanda);
        DbContext.MovimientosInventario.RemoveRange(DbContext.MovimientosInventario);
        DbContext.Notificaciones.RemoveRange(DbContext.Notificaciones);
        await DbContext.SaveChangesAsync();
        
        // 2. Limpiar entidades intermedias
        DbContext.Comandas.RemoveRange(DbContext.Comandas);
        DbContext.Reservaciones.RemoveRange(DbContext.Reservaciones);
        DbContext.Preparaciones.RemoveRange(DbContext.Preparaciones);
        DbContext.Facturas.RemoveRange(DbContext.Facturas);
        DbContext.TarjetasFidelizacion.RemoveRange(DbContext.TarjetasFidelizacion);
        DbContext.Promociones.RemoveRange(DbContext.Promociones);
        DbContext.OrdenesCompra.RemoveRange(DbContext.OrdenesCompra);
        DbContext.ContactosProveedor.RemoveRange(DbContext.ContactosProveedor);
        await DbContext.SaveChangesAsync();
        
        // 3. Limpiar entidades principales
        DbContext.Mesas.RemoveRange(DbContext.Mesas);
        DbContext.Clientes.RemoveRange(DbContext.Clientes);
        DbContext.Proveedores.RemoveRange(DbContext.Proveedores);
        DbContext.Ingredientes.RemoveRange(DbContext.Ingredientes);
        DbContext.Productos.RemoveRange(DbContext.Productos);
        DbContext.Usuarios.RemoveRange(DbContext.Usuarios);
        DbContext.ProductoCategorias.RemoveRange(DbContext.ProductoCategorias);
        await DbContext.SaveChangesAsync();
        
        // 4. Limpiar entidades de Identity (al final)
        DbContext.Set<ApplicationRole>().RemoveRange(DbContext.Set<ApplicationRole>());
        DbContext.Set<IdentityApplicationUser>().RemoveRange(DbContext.Set<IdentityApplicationUser>());
        await DbContext.SaveChangesAsync();
        
        Console.WriteLine("✅ Limpieza manual completada");
    }

    public virtual async Task DisposeAsync()
    {
        try
        {
            // Intentar limpiar la base de datos
            await ResetDatabaseAsync();
        }
        catch (Exception ex)
        {
            // Capturar cualquier error durante la limpieza y solo loguearlo
            // No permitir que errores de limpieza afecten el resultado de los tests
            Console.WriteLine($"⚠️ Advertencia: Error durante limpieza de BD: {ex.Message}");
        }
        finally
        {
            // Siempre liberar recursos del contexto
            try
            {
                DbContext?.Dispose();
            }
            catch { }
        }
    }
}

// Fakes para servicios de dominio
public class FakeDateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.Today;
}

public class FakeIdentityService : IIdentityService
{
    public Task<(Result Result, string UserId)> CreateUserAsync(string userName, string email, string password)
        => Task.FromResult((Result.Success(), "fake-user-id"));
    public Task<Result<string>> RegisterAsync(string nombre, string apellidos, string email, string username, string password, string rol)
        => Task.FromResult(Result<string>.Success("fake-user-id"));
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
    public Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string? confirmNewPassword = null)
        => Task.FromResult(Result.Success());
    public Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
        => Task.FromResult(Result<AuthResponse>.Success(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } }));
    public Task<Result<AuthResponse>> RefreshTokenAsync(string token, string refreshToken)
        => Task.FromResult(Result<AuthResponse>.Success(new AuthResponse { Success = true, Message = "OK", Token = "fake-token", Expiration = DateTime.UtcNow.AddHours(1), UserId = "fake-user-id", UserName = "FakeUser", Roles = new List<string> { "Admin" } }));
}

public class FakeJwtTokenService : IJwtTokenService
{
    public JwtTokenResponse GenerateToken(string userId, string userName, string email, IList<string> roles)
        => new JwtTokenResponse { AccessToken = "fake-jwt-token", TokenType = "Bearer", ExpiresIn = 3600, RequiresRefresh = false };
    public string GenerateRefreshToken() => "fake-refresh-token";
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token) => new ClaimsPrincipal();
}

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

public class FakeUserManager : Microsoft.AspNetCore.Identity.UserManager<RestaurantePro.Infrastructure.Identity.Models.ApplicationUser>
{
    public FakeUserManager() : base(
        Substitute.For<Microsoft.AspNetCore.Identity.IUserStore<RestaurantePro.Infrastructure.Identity.Models.ApplicationUser>>(),
        null, null, null, null, null, null, null, null)
    { }
}

// Si tienes un DbContext de test, agrega este método para entidades de prueba y keyless:
// protected override void OnModelCreating(ModelBuilder modelBuilder)
// {
//     base.OnModelCreating(modelBuilder);
//     modelBuilder.Entity<RestaurantePro.Domain.Core.Base.Events.DomainEvent>().HasNoKey();
//     modelBuilder.Entity<TestEntity>();
//     modelBuilder.Entity<NonAuditableTestEntity>();
// } 