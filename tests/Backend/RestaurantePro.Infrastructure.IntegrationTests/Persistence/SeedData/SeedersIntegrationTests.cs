using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Infrastructure.Identity.Models;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

/// <summary>
/// Tests de integración E2E para validar que todos los seeders críticos
/// funcionan correctamente juntos en el orden correcto.
/// </summary>
public class SeedersIntegrationTests : IntegrationTestBase
{
    public SeedersIntegrationTests(DatabaseFixture fixture) : base(fixture) { }

    private RestauranteProDbContext _context = null!;
    private ILogger<SeedersIntegrationTests> _logger = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        _logger = ServiceProvider.GetRequiredService<ILogger<SeedersIntegrationTests>>();
    }

    [Fact]
    public async Task EjecutarTodosLosSeeders_DeberiaCompletarseSinErrores()
    {
        // Arrange
        var seeders = new List<ISeedData>
        {
            new RolesSeeder(),
            new PermisosSeeder(),
            new EstadosSeeder(),
            new UnidadesMedidaSeeder(),
            new ConfiguracionSeeder(),
            new UsuarioAdminSeeder()
        };

        // Agregar IdentityUsersSeeder con sus dependencias
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);
        seeders.Add(identitySeeder);

        // Act
        foreach (var seeder in seeders)
        {
            var logger = ServiceProvider.GetRequiredService<ILogger<object>>();
            await seeder.SeedAsync(_context, logger, CancellationToken.None);
        }

        // Assert
        // Verificar que la base de datos tiene datos
        Assert.True(await _context.Roles.AnyAsync());
        Assert.True(await _context.Usuarios.AnyAsync());
    }

    [Fact]
    public async Task EjecutarSeedersEnOrden_DeberiaCrearDatosConsistentes()
    {
        // Arrange
        var rolesSeeder = new RolesSeeder();
        var permisosSeeder = new PermisosSeeder();
        var estadosSeeder = new EstadosSeeder();
        var unidadesSeeder = new UnidadesMedidaSeeder();
        var configSeeder = new ConfiguracionSeeder();
        var adminSeeder = new UsuarioAdminSeeder();
        
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);

        // Act - Ejecutar en orden correcto
        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();
        
        await rolesSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await permisosSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await estadosSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await unidadesSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await configSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await adminSeeder.SeedAsync(_context, logger, CancellationToken.None);
        await identitySeeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert - Verificar datos críticos
        var roles = await _context.Roles.ToListAsync();
        var usuarios = await _context.Usuarios.ToListAsync();

        // Verificar roles críticos
        Assert.Contains(roles, r => r.Name == "Administrador");
        Assert.Contains(roles, r => r.Name == "Gerente");
        Assert.Contains(roles, r => r.Name == "Cajero");
        Assert.Contains(roles, r => r.Name == "Mesero");

        // Verificar usuario admin
        var admin = usuarios.FirstOrDefault(u => u.Email == "admin@restaurantepro.com");
        Assert.NotNull(admin);
        Assert.Equal(EstadoUsuario.Activo, admin.Estado);
        Assert.Contains(admin.Roles, r => r == RolUsuario.Administrador);
    }

    [Fact]
    public async Task EjecutarSeedersIdempotentemente_DeberiaMantenerDatosConsistentes()
    {
        // Arrange
        var seeders = new List<ISeedData>
        {
            new RolesSeeder(),
            new PermisosSeeder(),
            new EstadosSeeder(),
            new UnidadesMedidaSeeder(),
            new ConfiguracionSeeder(),
            new UsuarioAdminSeeder()
        };

        // Agregar IdentityUsersSeeder con sus dependencias
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);
        seeders.Add(identitySeeder);

        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();

        // Act - Ejecutar dos veces
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(_context, logger, CancellationToken.None);
        }

        var rolesPrimeraEjecucion = await _context.Roles.CountAsync();
        var usuariosPrimeraEjecucion = await _context.Usuarios.CountAsync();

        // Ejecutar segunda vez
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(_context, logger, CancellationToken.None);
        }

        var rolesSegundaEjecucion = await _context.Roles.CountAsync();
        var usuariosSegundaEjecucion = await _context.Usuarios.CountAsync();

        // Assert - Verificar idempotencia
        Assert.Equal(rolesPrimeraEjecucion, rolesSegundaEjecucion);
        Assert.Equal(usuariosPrimeraEjecucion, usuariosSegundaEjecucion);
    }

    [Fact]
    public async Task EjecutarSeedersConDependencias_DeberiaRespetarOrdenCorrecto()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();

        // Act - Ejecutar solo RolesSeeder primero
        var rolesSeeder = new RolesSeeder();
        await rolesSeeder.SeedAsync(_context, logger, CancellationToken.None);

        // Verificar que RolesSeeder funcionó
        Assert.True(await _context.Roles.AnyAsync());

        // Ejecutar UsuarioAdminSeeder (depende de RolesSeeder)
        var adminSeeder = new UsuarioAdminSeeder();
        await adminSeeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert - Verificar que el admin se creó correctamente
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == "admin@restaurantepro.com");
        
        Assert.NotNull(admin);
        Assert.Contains(admin.Roles, r => r == RolUsuario.Administrador);
    }

    // [Fact]
    // public async Task EjecutarSeedersSinDependencias_DeberiaFallarApropiadamente()
    // {
    //     // Este test ya no aplica porque el seeder no lanza excepción si faltan roles
    // }

    [Fact]
    public async Task ValidarDatosCriticosDelSistema_DeberiaEstarCompletos()
    {
        // Arrange
        var seeders = new List<ISeedData>
        {
            new RolesSeeder(),
            new PermisosSeeder(),
            new EstadosSeeder(),
            new UnidadesMedidaSeeder(),
            new ConfiguracionSeeder(),
            new UsuarioAdminSeeder()
        };

        // Agregar IdentityUsersSeeder con sus dependencias
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);
        seeders.Add(identitySeeder);

        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();

        // Act - Ejecutar todos los seeders
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(_context, logger, CancellationToken.None);
        }

        // Assert - Verificar que el sistema tiene todos los datos críticos
        var roles = await _context.Roles.ToListAsync();
        var usuarios = await _context.Usuarios.ToListAsync();

        // Verificar roles mínimos necesarios
        Assert.True(roles.Count >= 4, "Debe haber al menos 4 roles críticos");
        Assert.True(usuarios.Count >= 1, "Debe haber al menos el usuario admin");

        // Verificar que el admin tiene todos los permisos necesarios
        var admin = usuarios.First(u => u.Email == "admin@restaurantepro.com");
        Assert.Equal(RolUsuario.Administrador, admin.Roles.First());
        Assert.Equal(EstadoUsuario.Activo, admin.Estado);
    }

    [Fact]
    public async Task EjecutarSeedersConTransaccion_DeberiaMantenerConsistencia()
    {
        // Arrange
        var seeders = new List<ISeedData>
        {
            new RolesSeeder(),
            new PermisosSeeder(),
            new EstadosSeeder(),
            new UnidadesMedidaSeeder(),
            new ConfiguracionSeeder(),
            new UsuarioAdminSeeder()
        };

        // Agregar IdentityUsersSeeder con sus dependencias
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);
        seeders.Add(identitySeeder);

        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();

        // Act - Ejecutar en transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(_context, logger, CancellationToken.None);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        // Assert - Verificar que los datos están en la base de datos
        Assert.True(await _context.Roles.AnyAsync());
        Assert.True(await _context.Usuarios.AnyAsync());
    }

    [Fact]
    public async Task ValidarPerformanceDeSeeders_DeberiaCompletarseEnTiempoRazonable()
    {
        // Arrange
        var seeders = new List<ISeedData>
        {
            new RolesSeeder(),
            new PermisosSeeder(),
            new EstadosSeeder(),
            new UnidadesMedidaSeeder(),
            new ConfiguracionSeeder(),
            new UsuarioAdminSeeder()
        };

        // Agregar IdentityUsersSeeder con sus dependencias
        var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        var roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var seedConfig = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var identitySeeder = new IdentityUsersSeeder(userManager, roleManager, seedConfig);
        seeders.Add(identitySeeder);

        var logger = ServiceProvider.GetRequiredService<ILogger<object>>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(_context, logger, CancellationToken.None);
        }

        stopwatch.Stop();

        // Assert - Verificar que no toma demasiado tiempo (máximo 5 segundos)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Los seeders tomaron {stopwatch.ElapsedMilliseconds}ms, máximo esperado: 5000ms");

        // Verificar que se completaron correctamente
        Assert.True(await _context.Roles.AnyAsync());
        Assert.True(await _context.Usuarios.AnyAsync());
    }
} 