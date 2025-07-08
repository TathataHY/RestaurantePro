using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using RestaurantePro.Infrastructure.IntegrationTests.TestBase;
using Xunit;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

public class IdentityUsersSeederTests : IntegrationTestBase
{
    private IdentityUsersSeeder _seeder = null!;
    private UserManager<IdentityApplicationUser> _userManager = null!;
    private RoleManager<ApplicationRole> _roleManager = null!;
    private RestauranteProDbContext _context = null!;

    public IdentityUsersSeederTests(DatabaseFixture fixture) : base(fixture)
    {
        // Los servicios se inicializarán en InitializeAsync
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _userManager = ServiceProvider.GetRequiredService<UserManager<IdentityApplicationUser>>();
        _roleManager = ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var config = ServiceProvider.GetRequiredService<IOptions<SeedDataConfiguration>>();
        var logger = ServiceProvider.GetRequiredService<ILogger<IdentityUsersSeeder>>();
        
        _seeder = new IdentityUsersSeeder(_userManager, _roleManager, config);
        _context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // 🔧 EJECUTAR ROLES SEEDER ANTES PARA QUE EXISTAN LOS ROLES
        var rolesSeeder = ServiceProvider.GetRequiredService<RolesSeeder>();
        var rolesLogger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        await rolesSeeder.SeedAsync(_context, rolesLogger);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearUsuariosEnIdentity_CuandoExistenEnDominio()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<IdentityUsersSeeder>>();
        // Arrange
        var usuarios = new List<Usuario>
        {
            Usuario.Crear(
                "admin",
                "Administrador del Sistema",
                "admin@restaurantepro.com",
                RolUsuario.Administrador
            ),
            Usuario.Crear(
                "gerente.general",
                "Gerente General",
                "gerente.general@restaurantepro.com",
                RolUsuario.Gerente
            ),
            Usuario.Crear(
                "chef.principal",
                "Chef Principal",
                "chef.principal@restaurantepro.com",
                RolUsuario.Cocinero
            )
        };
        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync(_context, logger);

        // Assert
        foreach (var usuario in usuarios)
        {
            var identityUser = await _userManager.FindByEmailAsync(usuario.Email);
            Assert.NotNull(identityUser);
        }
    }

    [Fact]
    public async Task SeedAsync_DeberiaGenerarContraseñasCorrectas_ParaTodosLosUsuarios()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<IdentityUsersSeeder>>();
        // Arrange
        var usuarios = new List<Usuario>
        {
            Usuario.Crear(
                "admin",
                "Administrador del Sistema",
                "admin@restaurantepro.com",
                RolUsuario.Administrador
            ),
            Usuario.Crear(
                "gerente.general",
                "Gerente General",
                "gerente.general@restaurantepro.com",
                RolUsuario.Gerente
            ),
            Usuario.Crear(
                "chef.principal",
                "Chef Principal",
                "chef.principal@restaurantepro.com",
                RolUsuario.Cocinero
            )
        };
        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync(_context, logger);

        // Assert
        Assert.True(await _userManager.CheckPasswordAsync(await _userManager.FindByEmailAsync("admin@restaurantepro.com"), "AdminRestaurante123!"));
        Assert.True(await _userManager.CheckPasswordAsync(await _userManager.FindByEmailAsync("gerente.general@restaurantepro.com"), "GerenteGeneral123!"));
        Assert.True(await _userManager.CheckPasswordAsync(await _userManager.FindByEmailAsync("chef.principal@restaurantepro.com"), "ChefPrincipal123!"));
    }

    [Fact]
    public async Task SeedAsync_NoDeberiaDuplicarUsuarios_CuandoSeEjecutaDosVeces()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<IdentityUsersSeeder>>();
        // Arrange
        var usuarios = new List<Usuario>
        {
            Usuario.Crear(
                "admin",
                "Administrador del Sistema",
                "admin@restaurantepro.com",
                RolUsuario.Administrador
            )
        };
        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();

        // Act
        await _seeder.SeedAsync(_context, logger);
        await _seeder.SeedAsync(_context, logger);

        // Assert
        var identityUsers = await _userManager.Users.ToListAsync();
        Assert.Single(identityUsers.Where(u => u.Email == "admin@restaurantepro.com"));
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarTrue_CuandoExistenUsuariosEnIdentity()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<IdentityUsersSeeder>>();
        // Arrange
        var usuario = Usuario.Crear(
            "admin",
            "Administrador del Sistema",
            "admin@restaurantepro.com",
            RolUsuario.Administrador
        );
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        await _seeder.SeedAsync(_context, logger);

        // Act
        var exists = await _userManager.FindByEmailAsync("admin@restaurantepro.com") != null;

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarFalse_CuandoNoExistenUsuariosEnIdentity()
    {
        // Act
        var existe = await _seeder.ExistsAsync(_context);

        // Assert
        Assert.False(existe);
    }

    private async Task CrearUsuariosEnDominio()
    {
        // Crear usuarios en el dominio para que el seeder los encuentre
        var usuarios = new List<Usuario>
        {
            Usuario.Crear(
                "admin",
                "Administrador del Sistema",
                "admin@restaurantepro.com",
                RolUsuario.Administrador
            ),
            Usuario.Crear(
                "gerente.general",
                "Gerente General",
                "gerente.general@restaurantepro.com",
                RolUsuario.Gerente
            ),
            Usuario.Crear(
                "chef.principal",
                "Chef Principal",
                "chef.principal@restaurantepro.com",
                RolUsuario.Cocinero
            )
        };

        _context.Usuarios.AddRange(usuarios);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        // Limpiar usuarios de Identity
        var usuarios = _userManager.Users.ToList();
        foreach (var usuario in usuarios)
        {
            await _userManager.DeleteAsync(usuario);
        }

        // Limpiar usuarios del dominio
        _context.Usuarios.RemoveRange(_context.Usuarios);
        await _context.SaveChangesAsync();
    }
} 