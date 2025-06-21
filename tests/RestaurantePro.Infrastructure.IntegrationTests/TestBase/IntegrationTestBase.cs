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

namespace RestaurantePro.Infrastructure.IntegrationTests.TestBase;

/// <summary>
/// Clase base para tests de integración de infraestructura
/// Proporciona una base de datos SQLite in-memory única para cada test
/// </summary>
    [Collection("DatabaseCollection")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly DatabaseFixture _fixture;
    protected RestauranteProDbContext DbContext = null!;
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
        
        // Registrar interceptores
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        
        // Registrar dispatcher de eventos de dominio
        services.AddScoped<IDomainEventDispatcher>(provider => 
            Substitute.For<IDomainEventDispatcher>());

        // Repositorios Core
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        // Repositorios Comercial
            services.AddScoped<IClienteRepository, ClienteRepository>();
        // Servicios de dominio y utilidades
        services.AddScoped<IDateTimeService, FakeDateTimeService>();
        services.AddScoped<IIdentityService, FakeIdentityService>();
        services.AddScoped<IJwtTokenService, FakeJwtTokenService>();
        services.AddScoped<IUserPermissionService, FakeUserPermissionService>();
    }

    /// <summary>
    /// Limpia la base de datos después de cada test
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        try
        {
            // 1. Limpiar entidades hijas primero (más dependientes)
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

            // 3. Limpiar entidades principales (menos dependientes)
            DbContext.Mesas.RemoveRange(DbContext.Mesas);
            DbContext.Clientes.RemoveRange(DbContext.Clientes);
            DbContext.Proveedores.RemoveRange(DbContext.Proveedores);
            DbContext.Ingredientes.RemoveRange(DbContext.Ingredientes);
            DbContext.Productos.RemoveRange(DbContext.Productos);
            DbContext.Usuarios.RemoveRange(DbContext.Usuarios);
            DbContext.ProductoCategorias.RemoveRange(DbContext.ProductoCategorias);
            await DbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Si hay error, intentar limpiar de forma más agresiva
            await CleanupDatabaseAggressively();
        }
    }

    /// <summary>
    /// Método de limpieza agresiva cuando el método normal falla
    /// </summary>
    private async Task CleanupDatabaseAggressively()
    {
        try
        {
            // Deshabilitar temporalmente las restricciones de clave foránea
            await DbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=OFF");
            
            // Limpiar todas las tablas en cualquier orden
            var entityTypes = DbContext.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                var tableName = entityType.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                {
                    await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\"");
                }
            }
            
            // Rehabilitar las restricciones
            await DbContext.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys=ON");
        }
        catch (Exception ex)
        {
            // Si todo falla, al menos intentar limpiar las tablas principales
            try
            {
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Usuarios\"");
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Productos\"");
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Clientes\"");
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM \"Mesas\"");
            }
            catch
            {
                // Si incluso esto falla, no hacer nada más
            }
        }
    }

    public virtual async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        DbContext?.Dispose();
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
    public Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
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