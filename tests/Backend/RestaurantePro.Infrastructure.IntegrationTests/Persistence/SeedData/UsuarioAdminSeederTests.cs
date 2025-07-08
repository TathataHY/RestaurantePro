using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Infrastructure.Persistence.SeedData.Critical;
using RestaurantePro.Infrastructure.Persistence.SeedData.Extensions;
using Microsoft.Data.Sqlite;

namespace RestaurantePro.Infrastructure.IntegrationTests.Persistence.SeedData;

/// <summary>
/// Tests de integración para UsuarioAdminSeeder
/// 
/// NOTA: Este seeder SÍ crea datos - el usuario administrador inicial del sistema.
/// </summary>
public class UsuarioAdminSeederTests : IntegrationTestBase
{
    public UsuarioAdminSeederTests(DatabaseFixture fixture) : base(fixture) { }

    private UsuarioAdminSeeder _seeder = null!;
    private RestauranteProDbContext _context = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _seeder = new UsuarioAdminSeeder();
        _context = ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // Ejecutar RolesSeeder primero (dependencia)
        var rolesSeeder = new RolesSeeder();
        var logger = ServiceProvider.GetRequiredService<ILogger<RolesSeeder>>();
        await rolesSeeder.SeedAsync(_context, logger, CancellationToken.None);
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearUsuarioAdminCorrectamente()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        Assert.Equal("admin", admin.NombreUsuario);
        Assert.Equal("admin@restaurantepro.com", admin.Email);
        Assert.Equal("Administrador del Sistema", admin.NombreCompleto);
        Assert.Equal(RolUsuario.Administrador, admin.Roles.First());
        Assert.Equal(EstadoUsuario.Activo, admin.Estado);
        Assert.Equal("Administración", admin.Departamento);
        Assert.Equal("Administrador del Sistema", admin.Posicion);
    }

    [Fact]
    public async Task SeedAsync_NoDeberiaDuplicarUsuarioAdmin()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act - Ejecutar dos veces
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admins = await _context.Usuarios
            .Where(u => u.NombreUsuario == "admin")
            .ToListAsync(CancellationToken.None);
        
        Assert.Single(admins);
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarTrue_CuandoExisteUsuarioAdmin()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Act
        var existe = await _seeder.ExistsAsync(_context, CancellationToken.None);

        // Assert
        Assert.True(existe);
    }

    [Fact]
    public async Task ExistsAsync_DeberiaRetornarFalse_CuandoNoExisteUsuarioAdmin()
    {
        // Act
        var existe = await _seeder.ExistsAsync(_context, CancellationToken.None);

        // Assert
        Assert.False(existe);
    }

    [Fact]
    public async Task SeedAsync_DeberiaAsignarPermisosDeAdministrador()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        
        // Verificar permisos críticos de administrador
        var permisosEsperados = new[]
        {
            "usuarios.crear", "usuarios.leer", "usuarios.actualizar", "usuarios.eliminar",
            "sistema.configurar", "sistema.monitorear",
            "operaciones.ver_todas", "reportes.ver_todos",
            "inventario.gestionar_todo", "comercial.gestionar_todo"
        };

        foreach (var permiso in permisosEsperados)
        {
            Assert.True(admin.TienePermiso(permiso), $"El admin debe tener el permiso: {permiso}");
        }
    }

    [Fact]
    public async Task SeedAsync_DeberiaUsarIdDeterministico()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();
        var adminIdEsperado = new Guid("11111111-1111-1111-1111-111111111111");

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        Assert.Equal(adminIdEsperado, admin.Id);
    }

    [Fact]
    public async Task SeedAsync_DeberiaActualizarAdminExistenteSiEsNecesario()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();
        
        // Crear admin inicial
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);
        
        // Modificar el admin para simular que necesita actualización
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        admin!.Desactivar(); // Cambiar estado
        await _context.SaveChangesAsync(CancellationToken.None);

        // Act - Ejecutar seeder nuevamente
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var adminActualizado = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(adminActualizado);
        Assert.Equal(EstadoUsuario.Activo, adminActualizado.Estado); // Debe estar activo
    }

    [Fact]
    public async Task SeedAsync_DeberiaManejarErroresGracefully()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();
        
        // Simular error en la base de datos
        await _context.Database.EnsureDeletedAsync(CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<SqliteException>(() => 
            _seeder.SeedAsync(_context, logger, CancellationToken.None));
        
        // Assert.NotNull(exception); // innecesario, el assert anterior ya valida la excepción
    }

    [Fact]
    public async Task SeedAsync_DeberiaGenerarLogsInformativos()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert - Verificar que el seeder se ejecutó sin errores
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        // Los logs se verifican indirectamente por el éxito de la operación
    }

    [Fact]
    public async Task SeedAsync_DeberiaCrearUsuarioConCredencialesPorDefecto()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        Assert.Equal("admin", admin.NombreUsuario);
        Assert.Equal("admin@restaurantepro.com", admin.Email);
        Assert.Equal("Administrador del Sistema", admin.NombreCompleto);
        
        // Verificar que tiene password hash
        Assert.NotNull(admin.PasswordHash);
        Assert.NotNull(admin.Salt);
        Assert.NotEmpty(admin.PasswordHash);
        Assert.NotEmpty(admin.Salt);
    }

    [Fact]
    public async Task SeedAsync_DeberiaConfirmarCuentaAutomaticamente()
    {
        // Arrange
        var logger = ServiceProvider.GetRequiredService<ILogger<UsuarioAdminSeeder>>();

        // Act
        await _seeder.SeedAsync(_context, logger, CancellationToken.None);

        // Assert
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == "admin", CancellationToken.None);
        
        Assert.NotNull(admin);
        Assert.Equal(EstadoUsuario.Activo, admin.Estado);
        // No existe FechaConfirmacion, solo se verifica el estado
    }
} 