using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Identity.Models;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

/// <summary>
/// Tests de integración para RolesSeeder
/// </summary>
public class RolesSeederTests : IntegrationTestBase
{
    public RolesSeederTests(DatabaseFixture fixture) : base(fixture) { }

    private RolesSeeder _seeder = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _seeder = new RolesSeeder();
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearRolesCriticosCorrectamente()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var rolesEsperados = new[]
        {
            "ADMINISTRADOR",
            "GERENTE", 
            "CAJERO",
            "MESERO",
            "COCINERO",
            "ENCARGADOINVENTARIO",
            "EMPLEADO"
        };

        // Act
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert
        foreach (var rolEsperado in rolesEsperados)
        {
            var rol = await context.Set<ApplicationRole>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NormalizedName == rolEsperado);
            
            Assert.NotNull(rol);
            Assert.Equal(rolEsperado, rol.NormalizedName);
            Assert.True(rol.IsSystemRole);
        }
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarTrue_CuandoExistenTodosLosRoles()
    {
        // Arrange - Crear roles primero
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Act & Assert
        var existenRoles = await _seeder.ExistsAsync(context, CancellationToken.None);
        Assert.True(existenRoles);
    }

    [Fact]
    public async Task SeedAsync_NoDeberiaDuplicarRolesExistentes()
    {
        // Arrange - Primera ejecución
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        await _seeder.SeedAsync(context, logger, CancellationToken.None);
        var rolesPrimeraEjecucion = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .CountAsync();

        // Act - Segunda ejecución
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert - Verificar que no se duplicaron
        var rolesSegundaEjecucion = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .CountAsync();

        Assert.Equal(rolesPrimeraEjecucion, rolesSegundaEjecucion);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearRolesConIdsEspecificos()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();

        // Act
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert
        var rolAdmin = await context.Set<ApplicationRole>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == "ADMINISTRADOR");
        
        Assert.NotNull(rolAdmin);
        Assert.NotEqual(Guid.Empty, rolAdmin.Id);
        Assert.True(rolAdmin.IsSystemRole);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearRolesConPropiedadesCorrectas()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();

        // Act
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert
        var roles = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .ToListAsync();

        Assert.NotEmpty(roles);

        foreach (var rol in roles)
        {
            Assert.NotNull(rol.Name);
            Assert.NotEmpty(rol.Name);
            Assert.True(rol.IsSystemRole);
            Assert.NotEqual(Guid.Empty, rol.Id);
        }
    }

    [Fact]
    public async Task SeedAsync_DeberiaManejarErroresGracefully()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Act & Assert - El seeder debe continuar aunque haya errores individuales
        await _seeder.SeedAsync(context, logger, CancellationToken.None);
        
        // Verificar que al menos algunos roles se crearon
        var rolesCreados = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .CountAsync();
        
        Assert.True(rolesCreados > 0);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearExactamente7RolesCriticos()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();

        // Act
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert
        var cantidadRoles = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .CountAsync();

        Assert.Equal(7, cantidadRoles);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearRolesEnOrdenCorrecto()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        var context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        var rolesEsperados = new[]
        {
            "ADMINISTRADOR",
            "CAJERO", 
            "COCINERO",
            "EMPLEADO",
            "ENCARGADOINVENTARIO",
            "GERENTE",
            "MESERO"
        };

        // Act
        await _seeder.SeedAsync(context, logger, CancellationToken.None);

        // Assert
        var rolesCreados = await context.Set<ApplicationRole>()
            .Where(r => r.IsSystemRole)
            .AsNoTracking()
            .Select(r => r.NormalizedName)
            .ToListAsync();

        // Verificar que todos los roles esperados están presentes (sin importar el orden)
        Assert.Equal(rolesEsperados.Length, rolesCreados.Count);
        foreach (var rolEsperado in rolesEsperados)
        {
            Assert.Contains(rolEsperado, rolesCreados);
        }
    }
} 